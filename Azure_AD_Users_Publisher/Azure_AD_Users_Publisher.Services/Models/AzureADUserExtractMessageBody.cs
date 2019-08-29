namespace Azure_AD_Users_Publisher.Services.Models
{
    // note: json currently receiving
    //{
    //"ID": "0e821c22-9749-44ce-bb3b-8031538a2cf1",
    //"FirstName": "David",
    //"LastName": "Gagnon",
    //"FranchiseNumber": "404",
    //"Title": "Office Coordinator",
    //"mail": "david.gagnon@homeinstead.com",
    //"Phone": "",
    //"Upn": "david.gagnon@homeinstead.com",
    //"Primary": "404"
    //}

    public class AzureADUserExtractMessageBody
    {
        public string ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FranchiseNumber { get; set; }
        public string Title { get; set; }
        public string mail { get; set; }
        public string Phone { get; set; }
        public string Upn { get; set; }
        public string Primary { get; set; }
    }
}
