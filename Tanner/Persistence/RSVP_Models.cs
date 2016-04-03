/*
 * TANNERBOT
 *
 * Persistence layer to keep track of completed dialogs 
 */
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Tanner.Persistence
{
    [Serializable]
    public class SingleRSVP
    {
        public Forms.Person person;
        public Forms.DinnerOption dinner_option;
    }

    [Serializable]
    public class UserContext
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Id { get; set; }
    }

    public class UserContextFactory
    {
        public static UserContext FromChannelAccount(ChannelAccount accountData)
        {
            if (accountData == null)
                return null;

            var context = new UserContext();

            if (accountData.ChannelId == "emulator")
            {
                context.Name = accountData.Name;
                context.PhoneNumber = "(555) 867 - 5309";
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

        public static async System.Threading.Tasks.Task<UserContext> FromPhoneNumber(string phoneNumber)
        {
            var items = await DocumentDBRepository<UserContext>.GetItemsAsync(uc => uc.PhoneNumber == phoneNumber);

            if (items.Count() == 0)
                return null;

            if (items.Count() != 1)
                throw new System.InvalidOperationException("Found multiple user contexts with the same phone number");
            
            return items.First();
        }
    }

    [Serializable]
    public class RSVP_Models
    {
        public List<SingleRSVP> rsvps;
    }
}