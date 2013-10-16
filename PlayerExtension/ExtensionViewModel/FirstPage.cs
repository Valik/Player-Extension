using PlayerExtension.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlayerExtension
{
    public class FirstPage
    {
        private Type mPage;

        internal FirstPage(Type page)
        {
            mPage = page;
        }        
        
        public Type page
        {
            get { return mPage; }
        }        
    }
}
