using AutoMapper;
using API.Mapping;

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("🔍 BẮT ĐẦU KIỂM TRA TỪNG PROFILE AUTO MAPPER...\n");
Console.ResetColor();

// Danh sách các profile cần kiểm tra
var profiles = new List<(string Name, Action<IMapperConfigurationExpression> Register)>
{
    ("MappingAccount", cfg => cfg.AddProfile<MappingAccount>()),
    ("MappingNotification", cfg => cfg.AddProfile<MappingNotification>()),
    ("MappingPolicy", cfg => cfg.AddProfile<MappingPolicy>()),
    ("MappingPost", cfg => cfg.AddProfile<MappingPost>()),
    ("MappingStartup", cfg => cfg.AddProfile<MappingStartup>()),
};

foreach (var (name, registerProfile) in profiles)
{
    try
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"▶️ Đang kiểm tra: {name}...");
        Console.ResetColor();

        var config = new MapperConfiguration(cfg =>
        {
            registerProfile(cfg);
        });

        config.AssertConfigurationIsValid();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✅ {name} HỢP LỆ.\n");
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ {name} BỊ LỖI:");
        Console.ResetColor();
        Console.WriteLine(ex.Message);
        Console.WriteLine("---- CHI TIẾT ----");
        Console.WriteLine(ex.InnerException?.Message ?? ex.StackTrace);
        Console.WriteLine();
    }
}

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("🟩 Hoàn tất kiểm tra tất cả profile.");
Console.ResetColor();
Console.WriteLine("Nhấn phím bất kỳ để thoát...");
Console.ReadKey();
