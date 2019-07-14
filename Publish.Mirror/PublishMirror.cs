using System;
using System.Linq;
using NLog;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.Extensibility;
using Tridion.ContentManager.Extensibility.Events;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.Publishing;

namespace Tridion.Events
{
    /// <summary>
    /// Publishing Mirror will mirror publishing performed in a list of source publications, to a list of publications defined 
    /// as target publications. Useful for implementations where items are published to multiple languages by rule. 
    /// </summary>
    [TcmExtension("Publishing Mirror Extension")]
    public class PublishMirror : TcmExtension
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public PublishMirror()
        {
            Subscribe();
        }

        public void Subscribe()
        {
            EventSystem.SubscribeAsync<RepositoryLocalObject, PublishOrUnPublishEventArgs>(PublishEvent, EventPhases.TransactionCommitted);
        }

        /// <summary>
        /// Publish Event handler which is triggered on publishing (or unpublishing) any repository local object. The handler
        /// will publish the item if published in a source publication to any publications defined as target publications.
        /// </summary>
        /// <param name="publishedItem">Item being published (page, component, category etc)</param>
        /// <param name="args">Publishing or unpublishing arguments</param>
        /// <param name="phase">event phase for the transaction</param>
        private void PublishEvent(RepositoryLocalObject publishedItem, PublishOrUnPublishEventArgs args, EventPhases phase)
        {
            var publication = publishedItem.ContextRepository as Publication;

            // Make sure the publication is in the list of publications where publishing should be mirrored
            if (Settings.SOURCE_PUBS.Any(p => p.Equals(publication?.Title) || p.Equals(publication?.Id)))
            {
                if (Settings.PUBLISH_LOGGING_ENABLED)
                {
                    logger.Debug("Publication title is in the list of source publications.");

                    var publishStatus = args is PublishEventArgs ? "Publish" : "Unpublish";
                    var publishEvent = args is PublishEventArgs ? args as PublishEventArgs : null;
                    var unpublishEvent = args is UnPublishEventArgs ? args as UnPublishEventArgs : null;

                    var creator = publishEvent != null ? 
                        publishEvent.PublishTransactions?.FirstOrDefault()?.Creator :
                        unpublishEvent?.PublishTransactions?.FirstOrDefault()?.Creator;

                    logger.Info(
                        $"{publishStatus} event " +
                        $"[initiated by]: {creator.Title} ({creator.Id}) -- " +
                        $"[publishing targets]: {args.Targets.Select(t => $"{t.Title} ({t.Id})")?.PrintList()} -- " +
                        $"[original publish item]: {publishedItem.Id} ({publishedItem.Title}) -- " +
                        $"[all {publishStatus} items (also published)]: {args.Items.Select(i => $"{i.Title} ({i.Id})")?.PrintList()}."
                    );
                }

                // Get the instruction & figure out if its a publish or an unpublish
                var publishUnpublishInstruction = TridionUtil.GetPublishOrUnpublishInstruction(args);

                // Get the publications for which publishing should be mirrored
                var mirrorPublications = TridionUtil.GetPublications(Settings.TARGET_PUBS, publishedItem.Session);

                if (Settings.PUBLISH_LOGGING_ENABLED) logger.Debug($"Found the list of mirror publications : {mirrorPublications.Select(p => p.Title + ", ")?.PrintList()} .");

                // Get the item which needs to be published in the mirrored publication
                var mirrorItems = TridionUtil.GetItemsInPublications(publishedItem, mirrorPublications?.Select(p => p.Id)?.ToList(), publishedItem.Session);

                if (Settings.PUBLISH_LOGGING_ENABLED) logger.Debug($"Mirroring {mirrorItems?.Count} publish items : {mirrorItems?.Select(i => i.Id + ", ")?.PrintList()}.");
                
                try
                {
                    // Publish the items to be mirrored in the mirrored publications
                    if (args.Targets.Count() > 0 && mirrorItems?.Count() > 0 && mirrorPublications?.Count() > 0)
                    {
                        var publishedItemIds = mirrorItems.Select(p => p.Id.ToString())?.PrintList(); 
                        var publicationTitles = mirrorPublications.Select(p => p.Title)?.PrintList();
                        var publishingTargetIds = args.Targets.Select(t => t.Id.ToString())?.PrintList();

                        var targetTypes = args.Targets.Select(t => t as TargetType);

                        if(targetTypes == null || targetTypes.Count() < 1)
                        {
                            if (Settings.PUBLISH_LOGGING_ENABLED) logger.Debug("No target types found. Returning.");

                            return;
                        }

                        if (args is PublishEventArgs)
                        {
                            if (Settings.PUBLISH_LOGGING_ENABLED) logger.Debug("Publishing event being mirrored.");

                            var publishInstruction = (PublishInstruction) publishUnpublishInstruction;
                            
                            if(Settings.FORCE_PUBLISH_CHILD_PUBS)
                            {
                                publishInstruction.ResolveInstruction.IncludeChildPublications = true;
                            }
                            if(Settings.FORCE_PUBLISH_MINOR_VERSION)
                            {
                                publishInstruction.ResolveInstruction.IncludeDynamicVersion = true;
                            }
                            if(Settings.FORCE_PUBLISH_WORKFLOW_VERSION)
                            {
                                publishInstruction.ResolveInstruction.IncludeWorkflow = true;
                            }

                            // if the setting is selected to mirror the transaction only if publish to children is selected, but the setting is false, exit.
                            if(Settings.MIRROR_IF_PROPOGATE_SELECTED && !publishInstruction.ResolveInstruction.IncludeChildPublications)
                            {
                                if (Settings.PUBLISH_LOGGING_ENABLED) logger.Debug("Exiting as mirror if propogate selected, but setting is false.");

                                return;
                            }

                            if (Settings.PUBLISH_LOGGING_ENABLED) logger.Info($"Mirroring publishing items '{publishedItemIds}' -- to publications {publicationTitles} -- to targets {targetTypes.Select(t => t.Title)?.PrintList()}.");

                            PublishEngine.Publish(mirrorItems, publishInstruction, targetTypes, PublishPriority.Low);
                        }
                        else if (args is UnPublishEventArgs)
                        {
                            if (Settings.PUBLISH_LOGGING_ENABLED) logger.Debug("Unpublishing event being mirrored.");

                            var unpublishInstruction = (UnPublishInstruction) publishUnpublishInstruction;

                            if (Settings.FORCE_PUBLISH_CHILD_PUBS)
                            {
                                unpublishInstruction.ResolveInstruction.IncludeChildPublications = true;
                            }

                            // if the setting is selected to mirror the transaction only if publish to children is selected, but the setting is false, exit.
                            if (Settings.MIRROR_IF_PROPOGATE_SELECTED && !unpublishInstruction.ResolveInstruction.IncludeChildPublications)
                            {
                                if (Settings.PUBLISH_LOGGING_ENABLED) logger.Debug("Exiting as mirror if propogate selected, but setting is false.");
                                return;
                            }

                            if (Settings.PUBLISH_LOGGING_ENABLED) logger.Info($"Mirroring unpublishing items '{publishedItemIds}' -- to publications {publicationTitles} -- to targets {publishingTargetIds}.");

                            PublishEngine.UnPublish(mirrorItems, unpublishInstruction, targetTypes, PublishPriority.Low);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"Error publishing items : {ex.Message} {ex.ToString()} {ex.StackTrace}");
                }
            }
        }
    }
}


