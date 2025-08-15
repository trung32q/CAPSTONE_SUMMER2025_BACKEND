using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using Infrastructure.Models;
using API.Mapping;

namespace UnitTest
{
    public static class TestHelper
    {
        public static IMapper CreateMapper()
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingAccount>();
                cfg.AddProfile<MappingNotification>();
                cfg.AddProfile<MappingPolicy>();
                cfg.AddProfile<MappingPost>();
                cfg.AddProfile<MappingStartup>();
            });
            return configuration.CreateMapper();
        }

        public static CAPSTONE_SUMMER2025Context CreateInMemoryDbContext()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddEntityFrameworkInMemoryDatabase();
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var optionsBuilder = new DbContextOptionsBuilder<CAPSTONE_SUMMER2025Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .UseInternalServiceProvider(serviceProvider);
            return new CAPSTONE_SUMMER2025Context(optionsBuilder.Options);
        }

        public static Account CreateTestAccount(int id, string email, string role = "STARTUP", string status = "VERIFIED")
        {
            return new Account
            {
                AccountId = id,
                Email = email,
                Role = role,
                Status = status,
                CreatedAt = DateTime.UtcNow,
                Password = BCrypt.Net.BCrypt.HashPassword("TestPassword123"),
                AccountProfile = new AccountProfile
                {
                    AccountId = id,
                    FirstName = "Test",
                    LastName = "User"
                }
            };
        }
    }
}