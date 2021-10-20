# Altura Dev Tools
A set of simple tools to help Artists and Developers using the Altura SmartNFTs to test their NFTs and make manual changes without the need for coding.

## Tools
- Altura Property Changer 📜
- Altura Image Changer 🖼
- Altura Air Dropper 🪂



![](https://i.imgur.com/PW3EdfW.png)

### Installation
Download the latest releases from [Releases](https://github.com/sdoddler/Altura-Dev-Tools/releases)
Or download and compile the source using Visual Studio.

## How to Use
Here I'll go over the main fields that are used in each tool and how to use them.

### Property Changer
This will change non-static properties on SmartNFTs, these properties **must** be set as non-static at minting. These properties can be changed by the creator of the NFT regardless of the wallet that owns them.

**Item Collection**: Collection the item is from.
**Specific Item**: Tick this if you only want to change the properties on 1 item, otherwise this change will apply to all items in a collection.
**Item Token**: If Specific Item is ticked, this field will specify which token from a collection you wish to modify. This can only be a number above 0.
**Property Name**: Name of the property you wish to change E.g. Luck
**Property Value**: Value to give to the property, these will always be converted to string values, but numbers will work.
**API Key**: The API Key assigned to you in the [Altura Developer Dashboard](https://altura-dev-portal.herokuapp.com/)

Once you fill in the above fields as required, and hit Change Property the program will check how many items you are trying to change and bring up a Message box to ensure you want to continue. If so click yes and the program will begin the process of changing this property via the Altura API. If the program runs into any errors it will stop and cancel incase of major issues.

### Image Changer
This will change the active image on SmartNFTs that chose "Use Altura API to change image" at minting. This image can be changed by the creator of the NFT regardless of the wallet that owns them.

**Item Collection**: Collection the item is from.
**Specific Item**: Tick this if you only want to change the image on 1 item, otherwise this change will apply to all items in a collection.
**Item Token**: If Specific Item is ticked, this field will specify which token from a collection you wish to modify. This can only be a number above 0.
**Image Index**: This relates to the Image index 
**API Key**: The API Key assigned to you in the [Altura Developer Dashboard](https://altura-dev-portal.herokuapp.com/)

Once you fill in the above fields as required, and hit Change Image the program will check how many items you are trying to change and bring up a Message box to ensure you want to continue. If so click yes and the program will begin the process of changing the image index via the Altura API. If the program runs into any errors it will stop and cancel incase of major issues.

### Air Dropper
This tool can be used to airdrop NFTs to individual users or holders of a specific Collection. The NFT you wish to transfer **MUST** be held in the wallet that is your private key in the Developer Dashboard. Also ensure there is enough BNB to cover the transfer/gas costs.

**Holder Collection**: For Automatic Air Drops - Check who holds any NFT in this collection
**Specific Item**: Tick this if you only want holders of one specific token to receive the airdrop
**Item Token**: If Specific Item is ticked, this field will specify which token from the holder collection you would like to target.
**Check Holders**: This will print a list of holders to the Log Box
**API Key**: The API Key assigned to you in the [Altura Developer Dashboard](https://altura-dev-portal.herokuapp.com/)
**Air Drop Collection**: Collection the item is from.
**Air Drop Token**: The Token from the Air Drop Collection you would like to transfer to holders
**Amount to AirDrop**: How many each holder will receive
**To Address**: If you only wish to drop to 1 person, you can manually put their address (0x1234) in this field and then hit Manual Airdrop.

Once you fill in the above fields as required, if you wish to do an automatic airdrop, hit the "Automatic Airdrop" button. This will check how many people you will send the NFT to, and confirm you wish to go ahead with a Yes/No messageBox. **On a failure the airdrop will cancel to avoid any unfortunate errors/issues.** To send an NFT to individual Addresses, use the Manual AirDrop Button, and the To Address field.


## Example
As an example to show how the Property Changer might be used, you can see below that I'm changing a property on a Shill Punk.

- For this I use the Shill punk Collection: 0x27970a7fa322bbfefe208dbca7f8130a964c2b12
- Specify the token I want: 93 (in this case  it refers to Shill Punk #92)
- Set the Property Name to Luck as this is what I want to change
- And the Property Value to Really Lucky

When I click Change Property a message box shows up - and confirms I'm happy to make this change. 

Upon clicking yes you can see the Log box shows "Updating Item: Shill Punk #92" and the Property "Luck" to the value "Really Lucky"

The Property is then immediately updated on the Altura API and website as you can see below.

![](https://i.imgur.com/AQGsrb8.png)


#### Support the Creator
If these tools were helpful to you please consider supporting me in one of the following ways
- Follow me on [Twitter](https://twitter.com/theshillverse)
- Checkout the [Shill Punk Army on Altura](https://app.alturanft.com/collection/0x27970a7fa322bbfefe208dbca7f8130a964c2b12)
- Send BSC Donations to 0xdda06a9d45d28a5ac74d5cfbe66c53d3cdf804cd
