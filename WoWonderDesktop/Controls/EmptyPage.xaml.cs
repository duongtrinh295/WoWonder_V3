using System.Windows.Controls;
using WoWonderDesktop.language;

namespace WoWonderDesktop.Controls
{
    /// <summary>
    /// Interaction logic for EmptyPage.xaml
    /// </summary>
    public partial class EmptyPage : UserControl
    {
        public EmptyPage()
        {
            InitializeComponent();
        }

        public enum Type
        {
            NoConnection,
            NoSearchResult,
            NoUsers,
            NoFollow,
            NoNearBy,
            NoEvent,
            NoSessions,
            NoBlockedUsers,
            NoCall,
            NoGroupChat,
            NoMessages,
            NoFriendRequests,
            NoGroupRequest,
            NoStartedMessages,
            NoPinnedMessages,
            NoArchive,
        }

        public void InflateLayout(Type type)
        {
            switch (type)
            {
                case Type.NoConnection:

                    EmptyIcon.Text = "\uf26d";

                    TitleText.Text = LocalResources.label5_NoConnection_TitleText;
                    DescriptionText.Text = LocalResources.label5_NoConnection_DescriptionText;
                       
                    break;
                case Type.NoSearchResult:

                    EmptyIcon.Text = "\uf375";

                    TitleText.Text = LocalResources.label5_NoSearchResult_TitleText;
                    DescriptionText.Text = LocalResources.label5_NoSearchResult_DescriptionText;

                    break;
                case Type.NoUsers:

                    EmptyIcon.Text = "\uf345";

                    TitleText.Text = LocalResources.label5_NoUsers_TitleText;
                    DescriptionText.Text = LocalResources.label5_NoUsers_DescriptionText;

                    break;
                case Type.NoFollow:

                    EmptyIcon.Text = "\uf345";

                    TitleText.Text = LocalResources.label5_NoFollow_TitleText;
                    DescriptionText.Text = LocalResources.label5_NoFollow_DescriptionText;

                    break;
                case Type.NoNearBy:

                    EmptyIcon.Text = "\uf345";

                    TitleText.Text = LocalResources.label5_NoNearBy_TitleText;
                    DescriptionText.Text = "";


                    break;
                case Type.NoEvent:

                    EmptyIcon.Text = "\uf2ab";

                    TitleText.Text = LocalResources.label5_NoEvent_TitleText;
                    DescriptionText.Text = LocalResources.label5_NoEvent_DescriptionText;
                    break;
                case Type.NoSessions:

                    EmptyIcon.Text = "\uf2ee";

                    TitleText.Text = LocalResources.label5_NoSessions_TitleText;
                    DescriptionText.Text = "";

                    break;
                case Type.NoBlockedUsers:

                    EmptyIcon.Text = "\uf345";

                    TitleText.Text = LocalResources.label5_NoBlockedUsers_TitleText;
                    DescriptionText.Text = LocalResources.label5_NoBlockedUsers_DescriptionText;

                    break;
                case Type.NoCall:

                    EmptyIcon.Text = "\uf2ac";

                    TitleText.Text = LocalResources.label5_NoCall_TitleText;
                    DescriptionText.Text = LocalResources.label5_NoCall_DescriptionText;

                    break;
                case Type.NoGroupChat:

                    EmptyIcon.Text = "\uf343";

                    TitleText.Text = LocalResources.label5_NoGroupChat_TitleText;
                    DescriptionText.Text = LocalResources.label5_NoGroupChat_DescriptionText;

                    break;
                case Type.NoMessages:

                    EmptyIcon.Text = "\uf2b7";

                    TitleText.Text = LocalResources.label5_NoMessages_TitleText;
                    DescriptionText.Text = LocalResources.label5_NoMessages_DescriptionText;

                    break; 
                case Type.NoFriendRequests:

                    EmptyIcon.Text = "\uf343";

                    TitleText.Text = LocalResources.label5_NoFriendRequests_TitleText;
                    DescriptionText.Text = "";

                    break;
                case Type.NoGroupRequest:

                    EmptyIcon.Text = "\uf343";

                    TitleText.Text = LocalResources.label5_NoGroupRequest_TitleText;
                    DescriptionText.Text = "";

                    break;
                case Type.NoStartedMessages:

                    EmptyIcon.Text = "\uf383";

                    TitleText.Text = LocalResources.label5_NoStartedMessages_TitleText;
                    DescriptionText.Text = "";

                    break;
                case Type.NoPinnedMessages:

                    EmptyIcon.Text = "\uf2b7";

                    TitleText.Text = LocalResources.label5_NoPinnedMessages_TitleText;
                    DescriptionText.Text = "";

                    break;
                case Type.NoArchive:

                    EmptyIcon.Text = "\uf27c";

                    TitleText.Text = LocalResources.label5_NoArchive_TitleText;
                    DescriptionText.Text = "";

                    break; 
            }
        }
    }
}
