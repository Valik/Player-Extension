using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlayerExtension.ExtensionModel
{
    public class TroubleWithMusicLibraryFolderEvent
    {
        private String mMessage;
        private String mFolder;


        public TroubleWithMusicLibraryFolderEvent(string message, string folder)
        {
            mMessage = message;
            mFolder = folder;
        }

        public String message
        {
            get { return mMessage; }
        }

        public String libraryFolder
        {
            get { return mFolder; }
        }
    }
}
