using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Contoso_Bank_Mike.Models;
using Microsoft.IdentityModel.Protocols;

namespace Contoso_Bank_Mike
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        /// 
        /// 
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

                string userMessag = activity.Text;

                StateClient stateClient = activity.GetStateClient();
                BotData userBotData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);

                string endResult = "Hello there Welcome to Contosomo! please type -->set name your_name<-- so we can remember you";
                string ResultName = "";
                Activity aMessage;

                if (userMessag.Length > 9)
                { 
                    if (userMessag.ToLower().Substring(0, 8).Equals("set name"))
                    {
                        string theUserName = userMessag.Substring(9);
                        userBotData.SetProperty<string>("UsersName", theUserName);
                        
                        ResultName = theUserName;
                        userBotData.SetProperty<bool>("UsernameSet", true);
                        await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userBotData);

                    }
                }




                ////
                /// Fake bank card
                /// 
                if (userMessag.ToLower().Equals("website"))
                {
                    Activity replyToConversation = activity.CreateReply("Contoso Banking");
                    replyToConversation.Recipient = activity.From;
                    replyToConversation.Type = "message";
                    replyToConversation.Attachments = new List<Attachment>();

                    List<CardImage> cardImages = new List<CardImage>();
                    cardImages.Add(new CardImage(url: "http://contosobankmike20161130054939.azurewebsites.net/images/Cotosologo.png"));

                    List<CardAction> cardButtons = new List<CardAction>();
                    CardAction plButton = new CardAction()
                    {
                        //just sends to my first MSA app seen as Cotoso bank doesn't exist :D
                        Value = "http://wackspeechplay.azurewebsites.net/",
                        Type = "openUrl",
                        Title = "Click for Contoso Website"
                    };
                    cardButtons.Add(plButton);

                    ThumbnailCard plCard = new ThumbnailCard()
                    {
                        Title = "Contoso Banking Website",
                        Subtitle = "Visit the Contoso Banking Website",
                        Images = cardImages,
                        Buttons = cardButtons
                    };

                    Attachment plAttachment = plCard.ToAttachment();
                    replyToConversation.Attachments.Add(plAttachment);
                    await connector.Conversations.SendToConversationAsync(replyToConversation);

                    return Request.CreateResponse(HttpStatusCode.OK);

                }








                   




                    if (userBotData.GetProperty<bool>("SentGreeting"))
                {
                    string help = " type -->help<-- for a list of commands";
                    string aUserName = userBotData.GetProperty<string>("UsersName");
                    ///just adding some fun to the auto response
                    string one = "Hello again, " + aUserName + " good to see you back!" + help;
                    string two = "WE MISSED YOU " + aUserName + " :D Glad your back!" + help;
                    string three = "Ohh, its you again " + aUserName + "! Good to see you" + help;
                    string four = "Back again ;) We love to see you " + aUserName + "!" + help;
                    string five = "Your back " + aUserName + ", Yaaaaay!" + help;

                    Random rand = new Random();
                    int dice = rand.Next(1, 5);

                    if (dice == 1)
                    {
                        endResult = one;
                    }
                    else if (dice == 2)
                    {
                        endResult = two;
                    }
                    else if (dice == 3)
                    {
                        endResult = three;
                    }
                    else if (dice == 4)
                    {
                        endResult = four;
                    }
                    else if (dice == 5)
                    {
                        endResult = five;
                    }
                }
                else
                {
                    userBotData.SetProperty<bool>("SentGreeting", true);
                    await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userBotData);
                }

                if (userMessag.ToLower().Equals("clear"))
                {
                    endResult = "User data has been cleared";
                    await stateClient.BotState.DeleteStateForUserAsync(activity.ChannelId, activity.From.Id);
                }

                if (userMessag.ToLower().Equals("that's not my name"))
                {
                    endResult = "Username has been cleared";
                    await stateClient.BotState.DeleteStateForUserAsync(activity.ChannelId, activity.From.Id);
                }


                if (userMessag.ToLower().Equals("help"))
                {
                    aMessage = activity.CreateReply($"command lists ignore arrows and lines ||set name -->Yournamehere<-- (sets name)|| website (loads website card)|| clear(wipes data) || newaccount (makes new account for user|| balance (checks account balance) || stock -->stockname eg msft<-- (checks stock price) || deposite -->amount<-- deposit into account || cashout -->amount <-- withdraw from account");
                    await connector.Conversations.ReplyToActivityAsync(aMessage);
                }






                if (userBotData.GetProperty<bool>("UsernameSet"))
                {



                    /// balance check
                    if (userMessag.ToLower().Equals("balance"))
                    {
                        string userName = userBotData.GetProperty<string>("UsersName");


                        ContosoAccounts user = new ContosoAccounts();

                        var usersList = await Database.DatabaseInstance.GetUser(userName);
                        foreach (ContosoAccounts u in usersList)
                        {
                            user.UserName = u.UserName;
                            user.Balance = u.Balance;
                            user.ID = u.ID;
                        }
                        aMessage = activity.CreateReply($"The current balance for {user.UserName} and the ID of {user.ID} is at {user.Balance}.");
                        await connector.Conversations.ReplyToActivityAsync(aMessage);
                    }





                    /// make a new account

                    if (userMessag.ToLower().Equals("newaccount"))
                    {

                        ContosoAccounts addUser = new ContosoAccounts()
                        {
                            UserName = userBotData.GetProperty<string>("UsersName"),
                            Balance = 0.0
                         };
                        await Database.DatabaseInstance.AddUser(addUser);
                        aMessage = activity.CreateReply($"Creating new account for {addUser.UserName} an account with the balance for {addUser.Balance}.");
                        await connector.Conversations.ReplyToActivityAsync(aMessage);
                    }



                    ///cash out
                    if (userMessag.ToLower().Substring(0, 7).Equals("cashOut"))
                    {
                        string placeHolder = userMessag.Substring(8);
                        double myCashOut = Convert.ToDouble(placeHolder);
                        string userName = userBotData.GetProperty<string>("UsersName");
                        var theUser = await Database.DatabaseInstance.GetUser(userName);
                        foreach (ContosoAccounts u in theUser)
                        {
                            u.UserName = userName;
                            u.Balance = u.Balance - myCashOut;
                            await Database.DatabaseInstance.UpdateUser(u);
                        }
                        aMessage = activity.CreateReply($"Withdrawing {myCashOut} from your account");
                        await connector.Conversations.ReplyToActivityAsync(aMessage);
                    }

                    ///cashin
                    if (userMessag.ToLower().Substring(0, 7).Equals("deposit"))
                    {
                        string anotherPlaceHolder = userMessag.Substring(8);
                        double myCashIn = Convert.ToDouble(anotherPlaceHolder);
                        string userName = userBotData.GetProperty<string>("UserName");
                        var theUser = await Database.DatabaseInstance.GetUser(userName);
                        foreach (ContosoAccounts u in theUser)
                        {
                            u.UserName = userName;
                            u.Balance = u.Balance + myCashIn;
                            await Database.DatabaseInstance.UpdateUser(u);
                        }
                        aMessage = activity.CreateReply($"Depositing {myCashIn} into your account");
                        await connector.Conversations.ReplyToActivityAsync(aMessage);
                    }

                }




                // Some checks to see if the message contains certain words so a response can be made
                string personCall = "person";
                string stockCall = "stock";

                bool p = userMessag.ToLower().Contains(personCall);
                bool s = userMessag.ToLower().Contains(stockCall);

                string costoso = null;
                string strRet = null;

    


                //  await connector.Conversations.ReplyToActivityWithHttpMessagesAsync(InfoReply);
                //HttpClient client = new HttpClient();
                //string costoso = string x = await client.GetStringAsync(new Uri("https://api.cognitive.microsoft.com/sts/v1.0" + activity.Text + ""));







                // calculate something for us to return
                int length = (activity.Text ?? string.Empty).Length;



                // return our reply to the user
                //  Activity reply = activity.CreateReply($"You sent {activity.Text} which was {length} characters");
               
                Activity Firstreply = activity.CreateReply(endResult);
                await connector.Conversations.ReplyToActivityAsync(Firstreply);


                // if the user asks about stock the a response will be added for the stock
                if (s)
                {
                    string strHolder = userMessag.ToLower();
                    // need to remove "stock" from the message so it can recognise the stock
                    strHolder = strHolder.Replace("stock", "");
                    strRet = await Contoso_Bank_Mike.StocksForMikesBank.GetStock(strHolder);
                    Activity theStockReply = activity.CreateReply(strRet);
                    await connector.Conversations.ReplyToActivityAsync(theStockReply);
                }


                // if the user mentions "person" then a response will be made saying that a person will get back to them
                if (p)
                {
                    costoso = "Hello there we have received your message at Contoso Bank and will get back to you as soon as possible! :)";
                    Activity reply = activity.CreateReply(costoso);
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
                

            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}