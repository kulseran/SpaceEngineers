using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace FSTC {

   public static class EventManager {
     public delegate void EventCallback(object data);

     struct EventDef {
       public long m_time;
       public EventCallback m_cb;
       public object m_cbData;
     }

     private static List<EventDef> m_events = new List<EventDef>(); 
     private static List<EventDef> m_pendingEvents = new List<EventDef>(); 
     private static bool m_dirty = false;

     /**
      * Add events to the event list. If an event is added while handling
      * an event, then it will at the soonest trigger next tick.
      */
     public static void AddEvent(long time, EventCallback cb) {
       EventDef def = new EventDef();
       def.m_time = time;
       def.m_cb = cb;
       def.m_cbData = null;
       m_pendingEvents.Add(def);
       m_dirty = true;
     }

     /**
      * Add events to the event list. If an event is added while handling
      * an event, then it will at the soonest trigger next tick.
      */
     public static void AddEvent(long time, EventCallback cb, object cbData) {
       EventDef def = new EventDef();
       def.m_time = time;
       def.m_cb = cb;
       def.m_cbData = cbData;
       m_pendingEvents.Add(def);
       m_dirty = true;
     }

     /**
      * Trigger all events whos timers have expired.
      */
     public static void TriggerEvents(long now) {
       if (m_dirty) {
         m_events.AddRange(m_pendingEvents);
         m_pendingEvents.Clear();
         m_events.Sort((x,y) => x.m_time.CompareTo(y.m_time));
         m_dirty = false;
       }

       int itr = 0;
       for (;itr < m_events.Count(); ++itr) {
         if (m_events[itr].m_time > now) {
           break;
         }
         m_events[itr].m_cb(m_events[itr].m_cbData);
       }
       if (itr > 0) {
         m_events.RemoveRange(0, itr);
       }
     }
   }
}