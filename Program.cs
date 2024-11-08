using Pulumi;
using Pulumi.Random;
using System.Collections.Generic;

return await Deployment.RunAsync(() =>
{
    var passwordLength = new DeferredOutput<int>();
    
    var firstComponent = new FirstComponent("firstComponent", new()
    {
        // late bound value, required from second component
        PasswordLength = passwordLength.Value
    });

    var secondComponent = new SecondComponent("secondComponent", new()
    {
        Username = firstComponent.Username
    });

    secondComponent.PasswordLength.Apply(length =>
    {
        passwordLength.Resolve(length * 2);
        return 0;
    });

    return new Dictionary<string, object?>
    {
        { "username", firstComponent.Username },
        { "password", firstComponent.Password },
        { "password-length", firstComponent.Password.Apply(x => x.Length) },
    };
});

public class FirstComponent : Pulumi.ComponentResource
{
    public class FirstComponentArgs
    {
        public Input<int> PasswordLength { get; set; } = null!;
    }

    public Output<string> Username { get; set; }
    public Output<string> Password { get; set; }
    public FirstComponent(string name, FirstComponentArgs args) 
        : base("components:index:FirstComponent", name)
    {
        // Resource A
        var username = new RandomPet($"{name}-username", new()
        {

        }, new CustomResourceOptions { Parent = this });

        // Resource B
        var password = new RandomPassword($"{name}-password", new()
        {
            Length = args.PasswordLength,
        }, new CustomResourceOptions { Parent = this });

        // from resourceA
        Username = username.Id;
        // from resourceB
        Password = password.Result;
    }
}

public class SecondComponent : Pulumi.ComponentResource
{
    public class SecondComponentArgs
    {
        public Output<string> Username { get; set; }
    }

    public Output<int> PasswordLength { get; set; }
    public SecondComponent(string name, SecondComponentArgs args)
        : base("components:index:SecondComponent", name)
    {
        // Resource C => depends on Resource A
        var username = new RandomPet($"{name}-username", new()
        {
            Length = args.Username.Apply(x => x.Length)
        }, new CustomResourceOptions { Parent = this });

        // Resource D => exports data that resource B depends on
        var password = new RandomPassword($"{name}-password", new()
        {
            Length = 16,
            Special = true,
            Numeric = false
        }, new CustomResourceOptions { Parent = this });

        // exporting the password length from resource
        this.PasswordLength = password.Length;
    }
}