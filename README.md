# Protoacme

Protoacme is a .NET Core 2.1 ACME client that leverages the ACME protocol from Lets Encrypt to issue and renew SSL certificates. This Api leverages the v2 endpoints from Lets Encrypt. 
Currently wildcard certificates are supported by the Lets Encrypt v2 endpoint but are not currently working through this Api. (should be working soon.)

## Getting Started

To get started, install the NuGet package in your project or download and compile the project in to your solution.

### Prerequisites

Protoacme requires .NET Core 2.1 or newer.

### Installing

Register the protoacme service in DI.

``` csharp
using Protoacme.Services;

...

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddProtoacme(options => 
            {
                //Uses the Lets Encrypt staging endpoint if an ASPNETCORE_ENVIRONMENT variable is set.
                if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")))
                    options.UseStaging = true;
            });
        }
```

## Examples

### Create an account in Lets Encrypt

``` csharp
//Create the account model
AcmeCreateAccount accountInfo = new AcmeCreateAccount();
accountInfo.Contact = new List<string>(){ "mailto:bob@toast.com" };
accountInfo.TermsOfServiceAgreed = true;

//Create the account in Lets Encrypt
AcmeAccount acmeAccount = await _protoacmeClient.Account.CreateAsync(accountInfo);

//Save the account info to disk for later use.
acmeAccount.SaveToFile(@"c:\temp\myaccount.dat");
```

### Create a new CSR

``` csharp
CSR csr = CertificateUtility.GenerateCsr(new List<string>(){ "domain1.com", "domain2.com" });
```

### Request a new certificate and challenge

``` csharp
var domains = new List<string>(){ "domain1.com", "domain2.com" }
AcmeCertificateRequest certRequest = new AcmeCertificateRequest();
foreach (var dnsName in domains)
{
    certRequest.Identifiers.Add(new DnsCertificateIdentifier() { Value = dnsName.Domain });
}

AcmeCertificateFulfillmentPromise promise = await _protoacmeClient.Certificate.RequestCertificateAsync(acmeAccount, certRequest);

ChallengeCollection challenges = await _protoacmeClient.Challenge.GetChallengesAsync(acmeAccount, promise, ChallengeType.Http);
```

### Setup your challenge
Depending on the challenge type you chose, you will need to setup your challenge so Lets Encrypt can verify that you own the domain name.
* Http Challenge
    * Place a file in your web server with the authorization key contents from the challenges response. The file should be located at .well-known/acme-challenge/{token from challenge response}

Currently, only http challenges are working. DNS and TLS Challenges coming soon.

### Verify your challenge with Lets Encrypt
``` csharp
foreach (var challenge in challenges)
{
    var startVerifyResult = await client.Challenge.ExecuteChallengeVerification(challenge);
    AcmeChallengeStatus challengeStatus = null;
    while (challengeStatus == null || challengeStatus.Status == "pending")
    {
        challengeStatus = await client.Challenge.GetChallengeVerificationStatus(challenge);
        //We need to wait at least 3 seconds before checking the status again.
        await Task.Delay(3000);
    }

    if (challengeStatus.Status != "valid")
    throw new Exception($"Failed to validate challenge token {challenge.Token}");
}
```

### Download your certificate
``` csharp
var cert = await client.Certificate.DownloadCertificateAsync(account, promise, csr, CertificateType.Cert);

//Save Certificate to file
using (FileStream fs = new FileStream(@"c:\temp\mycert.cer", FileMode.Create))
{
    byte[] buffer = cert.Array;
    fs.Write(buffer, 0, buffer.Length);
}
```

## Authors

* **Chris Bardsley** - *Via Writelines* - [WriteLinez](https://github.com/writelinez)

## License

This project is licensed under the MIT License

## Acknowledgments

* To the Lets Encrypt Team!  Support Lets Encrypt if you can so they can continue to provide free SSL certificates to the world!
* [Lets Encrypt](https://letsencrypt.org/)
