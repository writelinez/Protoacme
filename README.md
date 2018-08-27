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

## Built With

* [Dropwizard](http://www.dropwizard.io/1.0.2/docs/) - The web framework used
* [Maven](https://maven.apache.org/) - Dependency Management
* [ROME](https://rometools.github.io/rome/) - Used to generate RSS Feeds

## Contributing

Please read [CONTRIBUTING.md](https://gist.github.com/PurpleBooth/b24679402957c63ec426) for details on our code of conduct, and the process for submitting pull requests to us.

## Versioning

We use [SemVer](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](https://github.com/your/project/tags). 

## Authors

* **Billie Thompson** - *Initial work* - [PurpleBooth](https://github.com/PurpleBooth)

See also the list of [contributors](https://github.com/your/project/contributors) who participated in this project.

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgments

* Hat tip to anyone whose code was used
* Inspiration
* etc
