# CustomAuthSchema

This repo gives idea how to create custom auth schema using custom Authentication handler

## Authentication overview

This is the process of determining or validating the user identity.

### Jargons(Common terms used)
1. ClaimsPrincipal
    
    Claims principal is the owner of the i.e every user. in simple example is you & me.
2. ClaimsIdentity

    THis is similar to the identity we use in our day to day life. i.e. a Pan card, DL or any other identity card.
3. Claims

    Claims are the parameters which are used in the identity i.e. first name, Last name, DOB.

    **ClaimsPrincipal<=Identity<=Claims**


## Authentication Schema

In concept it is similar to data base schema so query could get
or update the data based on that. In DB schema gives the
structure of the data. so similarly in Auth schema it gives how
to handle that auth request.

Every auth schema is associated with a **auth schema handler** in simple words **name** of the schema always represents a handler. Schema handler's know how to deal with the schema i.e from where to get the credentials and how to process them and returns if the authentication is **success or failure**. In case of failure it can challenge the user.

### Simple custom auth schema & Handler sample.

Every custom authentication handler must inherits from **AuthenticationHandler<T>** here **T** is of type **AuthenticationSchemeOptions**

```csharp
public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        //User service will help to get the details and compaire if the user is valid or not.
        private readonly IUserService _userService;

        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IUserService userService)
            : base(options, logger, encoder, clock)
        {
            _userService = userService;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return AuthenticateResult.Fail("Missing Authorization Header");

            User user = null;
            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);
                var username = credentials[0];
                var password = credentials[1];
                user = await _userService.Authenticate(username, password);
            }
            catch
            {
                return AuthenticateResult.Fail("Invalid Authorization Header");
            }

            if (user == null)
                return AuthenticateResult.Fail("Invalid Username or Password");

            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
            };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            //Challenge the user if auth failed
            return base.HandleChallengeAsync(properties);
        }
```
To register the authentication you have to call the **AddAuthentication** method in service collection.
```csharp
services.AddAuthentication("MySchema")
        .AddScheme<AuthenticationSchemeOptions,BasicAuthenticationHandler>("MySchema", null);
```        
to activate authentication and authorization you should activate the middle ware in AspNetCore.
```csharp
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
```
### Simple custom auth schema & Handler sample with custom Authorization option.

Create a authentication option by inheriting from **AuthenticationSchemeOptions**. Is used in case you want to do the customization of your handler based on some user configuration.
```csharp
    public class CustomAuthenticationSchemeOptions : AuthenticationSchemeOptions
    {
        public bool CoolConfig { get; set; }
    }
```
Then create the handler passing the custom option.
```csharp
    public class BasicAuthenticationHandlerWithSchemaOption : AuthenticationHandler<CustomAuthenticationSchemeOptions>{

    }
```
so while registering the the authentication schema you can pass the authentication option. 
```csharp
services.AddAuthentication()
        .AddScheme<CustomAuthenticationSchemeOptions,
                    BasicAuthenticationHandlerWithSchemaOption>("MySchema1", options =>
                    {
                        options.CoolConfig = true;
                    })
```                                    
### How default schema behaves with Aspnet core.



