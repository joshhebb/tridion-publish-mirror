using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Tridion.ContentManager;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.Extensibility.Events;
using Tridion.ContentManager.Publishing;
using NLog;

namespace Tridion.Events
{
    /// <summary>
    /// Tridion Hlper Functions 
    /// </summary>
    static class TridionUtil
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Figure 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static PublishInstructionBase GetPublishOrUnpublishInstruction(PublishOrUnPublishEventArgs args)
        {
            PublishInstructionBase publishInstruction = null;

            if (args is PublishEventArgs)
            {
                publishInstruction = ((PublishEventArgs) args).PublishInstruction;
            }
            else if (args is UnPublishEventArgs)
            {
                publishInstruction = ((UnPublishEventArgs) args).UnPublishInstruction;
            }

            return publishInstruction;
        }

        /// <summary>
        /// When an item is published, it is in the context of the publication which it was published from. In order to publish the item in the
        /// "mirrored" publications - we need to retrieve the item in each of the target publications.
        /// </summary>
        /// <param name="item">item published originally by the user</param>
        /// <param name="publications">list of publications which the item should also be published to</param>
        /// <returns></returns>
        public static List<IdentifiableObject> GetItemsInPublications(IdentifiableObject item, List<TcmUri> publicationIds, Session session)
        {
            if(item == null)
            {
                return null;
            }

            // For each of the publications we should mirror publishing to, create the ID for the item in that publication and query it.
            return publicationIds?.Select(p =>
            {
                var tcmUri = new TcmUri(item.Id.ItemId, item.Id.ItemType, p.ItemId);
                IdentifiableObject result = null;

                try
                {
                    if (session.IsExistingObject(tcmUri.ToString()))
                    {
                        result = session.GetObject(tcmUri.ToString());
                    }
                }
                catch(Exception ex)
                {
                    logger.Error($"Error {ex.Message} -- {ex.ToString()}");
                }
                
                return result;
            })?.ToList();
        }

        /// <summary>
        /// Returns a list of publications specified by publication titles, or TCM Ids.
        /// </summary>
        /// <param name="publicationIdentifiers">string array of publication titles or IDs</param>
        /// <param name="session"></param>
        /// <returns></returns>
        public static List<Publication> GetPublications(string[] publicationIdentifiers, Session session)
        {
            List<Publication> publications = new List<Publication>();
            try
            {
                PublicationsFilter filter = new PublicationsFilter(session);

                // Get the list of publications and find them by title
                foreach (XmlNode pubXml in session.GetList(filter))
                {
                    // search for the specified publications, and for each query the publication data and add it to the return list
                    publicationIdentifiers?.Where(p => p.Equals(pubXml.Attributes[Settings.Constants.CM_XML_TITLE].Value) || p.Equals(pubXml.Attributes[Settings.Constants.CM_XML_ID].Value))?
                        .ToList().ForEach(p =>
                        {
                            publications.Add(session.GetObject(pubXml.Attributes[Settings.Constants.CM_XML_ID].Value) as Publication);
                        });
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error getting publication data for pubs to mirror : {ex.Message} {ex.ToString()}");
            }

            return publications;
        }
    }
}
