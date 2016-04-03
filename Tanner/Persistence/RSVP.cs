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
        public string Name;
        public string PhoneNumber;

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
    }

    [Serializable]
    public class RSVP
    {
        public List<SingleRSVP> rsvps;
    }
}