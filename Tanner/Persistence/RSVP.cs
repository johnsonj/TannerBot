/*
 * TANNERBOT
 *
 * Persistence layer to keep track of completed dialogs 
 */
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
    public class RSVP
    {
        public List<SingleRSVP> rsvps;
    }
}