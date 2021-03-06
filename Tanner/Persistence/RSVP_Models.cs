﻿/*
 * TANNERBOT
 *
 * Persistence layer to keep track of completed dialogs 
 */
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Tanner.Persistence
{
    [Serializable]
    public class SingleRSVP
    {
        public Forms.SinglePersonRSVP Person;
        public Forms.DinnerOption DinnerOption;
    }

    [Serializable]
    public class UserContext
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Id { get; set; }
        public string ChannelAccountId { get; set; }
        public SingleRSVP MainRSVP { get; set; }

        // HACK: Ideally we'd do something smart with this, like a list of RSVPs but the wedding is fast approaching. 
        // Not all guests get a +1
        public bool GuestAllotted { get; set; }
        public SingleRSVP GuestRSVP { get; set; }
    }

    public class UserContextFactory
    {
        public static async Task<UserContext> EnsureFromChannelAccount(ChannelAccount accountData)
        {
            // Try to find by ChannelAccountId, it should be reliable
            var lookup = await FromChannelAccountId(accountData.Id);
            if (lookup != null)
                return lookup;

            // Try address, hope its a phone number!
            lookup = await FromPhoneNumber(accountData.Address, accountData.Id);

            if (lookup != null)
                return lookup;

            // Locally test known users
            if (accountData.ChannelId == "emulator" && accountData.Name == "User1")
            {
                lookup = await FromPhoneNumber("+15555555555", null /*channelId*/);
                if (lookup != null)
                    return lookup;
            }

            // Not found, need to create it
            return CreateFromChannelAccount(accountData);
        }
        public static UserContext CreateFromChannelAccount(ChannelAccount accountData)
        {
            if (accountData == null)
                return null;

            var context = new UserContext();
            context.ChannelAccountId = accountData.Id;
  
            // TODO: Makes the dev a bit easier but maybe remove in prod
            if (accountData.ChannelId == "emulator")
            {
                context.Name = accountData.Name;
                context.PhoneNumber = "+15558675309";
                return context;
            }
            else if (accountData.ChannelId == "sms")
            {
                // TODO: is name useful here? ideally we will look up the cell number in the db of
                // known guests.

                // Name is sometimes, or always the phone number
                if (accountData.Name != accountData.Address)
                    context.Name = accountData.Name;
   
                context.PhoneNumber = accountData.Address;
                return context;
            }

            return null;
        }

        public static async Task<UserContext> FromPhoneNumber(string phoneNumber, string channelId)
        {
            // TODO: GetItemsAsync doesn't work with a real predicate
            var all_items = await DocumentDBRepository<UserContext>.GetItemsAsync(obj => true);
            var search_item = all_items.FirstOrDefault(obj => obj.PhoneNumber.Equals(phoneNumber));

            if (search_item != null)
            {
                // Update the found item with the current channelId for lookup later
                if (channelId != null)
                {
                    search_item.ChannelAccountId = channelId;
                    await EnsurePresisted(search_item);
                }
                return search_item;
            }

            return null;
        }

        public static async Task<UserContext> FromChannelAccountId(string channelId)
        {
            // TODO: GetItemsAsync doesn't work with a real predicate
            var all_items = await DocumentDBRepository<UserContext>.GetItemsAsync(obj => true);
            var search_item = all_items.FirstOrDefault(obj => obj.ChannelAccountId != null && obj.ChannelAccountId.Equals(channelId));

            if (search_item != null)
                return search_item;

            return null;
        }

        // HACK: No idea where this sort of code should live
        public static async Task<Microsoft.Azure.Documents.Document> EnsurePresisted(UserContext obj)
        {
            if (obj == null)
                new System.InvalidOperationException("Blank user context provided");

            UserContext persisted_obj = null;
            if (obj.Id != null)
                persisted_obj = await DocumentDBRepository<UserContext>.GetItemAsync(obj.Id);

            if (persisted_obj == null)
            {
                // Need to create this
                return await DocumentDBRepository<UserContext>.CreateItemAsync(obj);
            }
            else
            {
                return await DocumentDBRepository<UserContext>.UpdateItemAsync(obj.Id, obj);
            }
        }
    }


    [Serializable]
    public class RSVP_Models
    {
        public List<SingleRSVP> rsvps;
    }
}