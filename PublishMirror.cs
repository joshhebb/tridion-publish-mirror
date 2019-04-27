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
            logger.Debug("Subscribing to publish & unpublish events..");
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
            if (Settings.SOURCE_PUBS.Any(p => p.Equals(publication?.Title)))
            {
                // Get the instruction & figure out if its a publish or an unpublish
                var publishUnpublishInstruction = TridionUtil.GetPublishOrUnpublishInstruction(args);

                // Only run the action if the user selected to include child pubs, and the config is enabled
                if (publishUnpublishInstruction.ResolveInstruction.IncludeChildPublications != true && Settings.OnlyMirrorIfPublishToChildrenSelected == true)
                {
                    logger.Debug("Transaction didn't specify that we should only publish if child publications setting is selected. Exitting.");
                    return;
                }
                else
                {
                    logger.Debug("Publishing transaction didn't specify to publish to child publications or the setting onlyPublishIfChildSelection was set to false. Continuing.");
                }

                // Get the publications for which publishing should be mirrored
                var mirrorPublications = TridionUtil.GetPublications(Settings.TARGET_PUBS, publishedItem.Session);

                // Get the item which needs to be published in the mirrored publication
                var mirrorItems = TridionUtil.GetItemsInPublications(publishedItem, mirrorPublications?.Select(p => p.Id)?.ToList(), publishedItem.Session);

                try
                {
                    // Publish the items to be mirrored in the mirrored publications
                    if (args.Targets.Count() > 0 && mirrorItems?.Count() > 0 && mirrorPublications?.Count() > 0)
                    {
                        var publishedItemIds = mirrorItems.Select(p => p.Id.ToString()).PrintList(); 
                        var publicationTitles = mirrorPublications.Select(p => p.Title).PrintList();
                        var publishingTargetIds = args.Targets.Select(t => t.Id.ToString()).PrintList();

                        var targetTypes = args.Targets.Select(t => t as TargetType);

                        if(targetTypes == null || targetTypes.Count() < 1)
                        {
                            return;
                        }

                        if (args is PublishEventArgs)
                        {
                            logger.Info($"Publishing items '{publishedItemIds}' -- to publications {publicationTitles} -- to targets {targetTypes.Select(t => t.Title).PrintList()}.");
                            PublishEngine.Publish(mirrorItems, (PublishInstruction) publishUnpublishInstruction, targetTypes);
                        }
                        else if (args is UnPublishEventArgs)
                        {
                            logger.Info($"Unpublishing items '{publishedItemIds}' -- to publications {publicationTitles} -- to targets {publishingTargetIds}.");
                            PublishEngine.UnPublish(mirrorItems, (UnPublishInstruction) publishUnpublishInstruction, targetTypes);
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


