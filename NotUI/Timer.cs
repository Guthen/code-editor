using System;
using System.Collections.Generic;
using System.Text;

namespace CodeEditor.NotUI
{
    class Timer
    {
        public string Name;
        public float TimeLeft;
        public Action Action;

        public static Dictionary<string, Timer> Timers = new Dictionary<string, Timer>();
        public static void UpdateAll( float dt )
        {
            foreach ( Timer timer in Timers.Values )
                timer.Update( dt );
        }

        public Timer( string name, float time, Action action )
        {
            Name = name;
            TimeLeft = time;
            Action = action;

            if ( Timers.ContainsKey( name ) )
                Timers[name] = this;
            else
                Timers.Add( name, this );
        }

        public void Update( float dt )
        {
            TimeLeft -= dt;
            if ( TimeLeft <= 0 )
            {
                Action();
                Timers.Remove( Name );
            }
        }
    }
}
