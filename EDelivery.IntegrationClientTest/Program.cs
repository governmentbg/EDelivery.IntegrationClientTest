using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.IntegrationClientTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //if you do not have STAMPIT root certificates in your trusted root certificates store,
            //please uncomment these rows
            //ServicePointManager.ServerCertificateValidationCallback +=
            //    (sender, cert, chain, sslPolicyErrors) => true;
            
            var service = GetService();

            var institutions = service.GetRegisteredInstitutions();
            Console.WriteLine(String.Join("\n",institutions.Select(i => i.Name)));
            Console.WriteLine("*************************************************");

            var sentMessageId = service.SendElectronicDocument(
                                "Test integration",
                                 System.Text.Encoding.UTF8.GetBytes("This is a test integration document!"),
                                 "Document test.txt",
                                "QEDJE-DSSS",
                                IntegrationService.eProfileType.Person,
                                "6801173680",
                                null,
                                "natalylazarova85@gmail.com",
                                serviceOID: null, //send null if service OID is not available
                                operatorEGN: null);
            Console.WriteLine("Sent message id: " + sentMessageId);            
            Console.WriteLine("*************************************************");

            var nextSentMessageId = service.SendMessage(new IntegrationService.DcMessageDetails()
                                                        {
                                                            Title = "This is title of test int.message",
                                                            MessageText = "Text of second text message",
                                                            AttachedDocuments = new IntegrationService.DcDocument[]
                                                            {
                                                                new IntegrationService.DcDocument()
                                                                {
                                                                    Content = System.IO.File.ReadAllBytes("script.sql"),
                                                                    ContentType = "application/text",
                                                                    DocumentName = "scritp.sql",
                                                                    DocumentRegistrationNumber="1111-x434243"                                                                   
                                                                },
                                                                new IntegrationService.DcDocument()
                                                                {
                                                                    Content = System.IO.File.ReadAllBytes("test.png"),
                                                                    ContentType = "image/png",
                                                                    DocumentName = "test.png",
                                                                    DocumentRegistrationNumber="2222-x434243"        
                                                                }
                                                            }
                                                        }, IntegrationService.eProfileType.Person, "8508033256",
                                                        null,
                                                        null,
                                                        serviceOID: "2.16.100.1.1.1.1.13.1.1.1", //provide a service OID if any
                                                        operatorEGN: null);
            Console.WriteLine("Sent message with 2 documents id: " + nextSentMessageId);
            Console.WriteLine("*************************************************");

            var result = service.GetSentDocumentStatusByRegNum("QEDJE-DSSS", null);
            if (result == null)
            {
                Console.WriteLine("Message with reg num QEDJE-DSSS is not fount");
            }
            else
            {
                Console.WriteLine("Message with reg num QEDJE-DSSS " + (result.DateReceived.HasValue?" is received on "+result.DateReceived.ToString():"is still not received"));
                
            }
            Console.WriteLine("*************************************************");

            var resultSentMessage = service.GetSentMessageStatus(sentMessageId, null);
            if (resultSentMessage == null)
            {
                Console.WriteLine("Message with id " +sentMessageId+" is not fount");
            }
            else
            {
                Console.WriteLine("Message with id " + sentMessageId + (result.DateReceived.HasValue ? " is received on " + result.DateReceived.ToString() : " is still not received"));
               
            } 
            Console.WriteLine("*************************************************");

            var sentDocumentContentByRegNum = service.GetSentDocumentContentByRegNum("QEDJE-DSSS", null);
            if (sentDocumentContentByRegNum == null)
            {
                Console.WriteLine("Document with registration number QEDJE-DSSS is not fount");
            }
            else
            {
                Console.WriteLine(String.Format("Document with registration number QEDJE-DSSS has name: {0} and content: {1}", sentDocumentContentByRegNum.DocumentName, System.Text.Encoding.UTF8.GetString(sentDocumentContentByRegNum.Content)));
                
            }
            Console.WriteLine("*************************************************");

            var sentMessagesList = service.GetSentMessagesList(null);
            if(sentMessagesList!=null)
            {
                Console.WriteLine("Sent messages:");
                foreach(var m in sentMessagesList)
                {
                    Console.WriteLine(String.Format("Title: {0}, Sent to: {1}, Send on: {2}, Is Received: {3} {4}",
                        m.Title, m.ReceiverProfile.ElectronicSubjectName, m.DateSent.Value, m.DateReceived.HasValue, (m.DateReceived.HasValue?m.DateReceived.Value.ToString():string.Empty)));

                }

            }
            else
            {
                Console.WriteLine("No sent messages!");
            }            
            Console.WriteLine("*************************************************");

            var receivedMessagesList = service.GetReceivedMessagesList(false,null);
            if(receivedMessagesList!=null)
            {
                Console.WriteLine("Received messages:");
                foreach (var m in receivedMessagesList)
                {
                    Console.WriteLine(String.Format("Title: {0}, Sent from: {1}, Send on: {2}, Is New: {3} {4}",
                        m.Title, m.SenderProfile.ElectronicSubjectName, m.DateSent.Value, !m.DateReceived.HasValue, (m.DateReceived.HasValue?m.DateReceived.Value.ToString():string.Empty)));

                }

            }
            else
            {
                Console.WriteLine("No received messages!");
            }            
            Console.WriteLine("*************************************************");

            var receivedMessageContent = service.GetReceivedMessageContent(receivedMessagesList.First().Id, null);
            if(receivedMessageContent!=null)
            {
                Console.WriteLine("Received message content:");
                Console.WriteLine("Title {0}, Message Text: {1}, Documents: {2}",
                    receivedMessageContent.Title, receivedMessageContent.MessageText, String.Join(";" ,receivedMessageContent.AttachedDocuments.Select(x=>x.DocumentName)));
            }
            
            Console.WriteLine("*************************************************");

            try
            {
               var unauth = service.GetReceivedMessagesList(true, "1234567898");
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }


            try
            {
                var unauth = service.GetReceivedMessagesList(true, "8508033256");
                Console.WriteLine("Success");
            }               
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadKey();
        }

        /// <summary>
        /// Get a service reference with the needed certificate for authentication
        /// </summary>
        /// <returns></returns>
        private static EDelivery.IntegrationClientTest.IntegrationService.EDeliveryIntegrationServiceClient GetService()
        {
            var service = new IntegrationService.EDeliveryIntegrationServiceClient();
            service.ClientCredentials.ClientCertificate.Certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(@"C:\Users\nataly\Desktop\x11.pfx", "1234");
            return service;

        }

    }
}
