# Viewer data API client

## Summary 
This .Net project You can use this to extract your cash counting data from the cloud for your own purposes.

## How to use

### Add project to your solution
**Clone the project** to your local machine and add it to your .Net Solution and add the reference **'ViewerApiClientSDK'** to the references for your project
### Add the 'using' reference to the name space **'Tellermate.ViewerApiClientSDK'**

`using Tellermate.ViewerApiClientSDK;`

### The simplest code to get some count data

Tellermate will have supplied a user ID and an API access key. Both of these will be in the Guid format. The ID is associated with your company and cannot be changed whereas the access key can be reset should it ever become compromised. Using the supplied information you can create a `Credentials` object. See below: 

            tm.Credentials creds = new Credentials()
            {
                ID = new Guid("dc09a1ee-799b-4d4d-b33c-7080c09daebd"),
                Key = new Guid("2f1d1241-ce0e-4b73-a8af-dad1f2cd1a7f"),
            };

Next create an instance of the `ViewerApiClient` and pass it the credential object 

     tm.ViewerApiClient Vclient = new tm.ViewerApiClient(creds);

You can now start accessing the data via the API

All the API calls return a `ClientResult `object. A `ClientResult` is made of three main parts.
1. The root of the data you are requesting is returned in the `ObjectValue` propery
2. The outcome of the communication with the server is returned in the `RequestStatus`. You will want to see a `RequestStatus.OK` but there are other states. You should only ever see the ones in **bold**, however if you see any of the others then please let us know, so we can investigate.
     1. **OK**- The data was retrieved successfully
     2. BadToken - Token was not in the correct format
     3. NoToken - The token had not been set
     4. **Unathorized**- Credentials were not correct. Check them and try again. If you can't figure it out contact us.
     5. **Other** - A response from the server that was neither OK or Unauthorized 
     6. ApiCodeError - Code Error in the API on the server.
     7. TooManyTries - Tried to connect more than 3 times.
     8. ClientCodeError - Code error in the Client (Please let us know, so we can investigate to improve)
3. The `Message` property contains various extra information to help with debugging. For example if you get a `RequestStatus.Other` the exact server response will be detailed in this property.

## The following API endpoints are available



**getCounts** - The following example retrieves all the count data from between 1st-Jan-2015 and today and places value in CountDataRoot 

     ClientResult<RootOrg> data = client.getCounts(new DateTime(2015, 1, 1), DateTime.Now);
     If(data.RequestStatus == RequestStatus.OK)
     {
         RootOrg CountDataRoot = data.ObjectValue;
          //Do stuff with data
     }

     else
     {
          // Handle error
     }

**getMachineStores** - for a lits of stores with their associated touch units

    ClientResult<List<MachineStore>> data = client.getMachineStores();

**getCountTypes** - for a list of Tellermate count types. Use this list to return full information on the count type. The cashcount API endpoint only returns the count type ID.

    ClientResult<List<CountType>> data = client.getCountTypes();
    
 ## Naritave Call Back   
    
There is also an option to send ad a function reference to every get a step by step description of what the Client is doing. 

**For example**

     ClientResult<List<MachineStore>> data = client.getMachineStores(MyFunction);
     
     
     public MyFunction(string WhatHappened)
     {
     GlobalLogString += WhatHappened;
     }
