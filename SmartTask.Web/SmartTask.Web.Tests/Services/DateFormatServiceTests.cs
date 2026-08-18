using Moq;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Services.Implementations;
using Xunit;

namespace SmartTask.Web.Tests.Services;

public class DateFormatServiceTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock;

    public DateFormatServiceTests()
    {
        _currentUserMock = new Mock<ICurrentUserService>();
    }

    private DateFormatService CreateService(DateFormatType format)
    {
        _currentUserMock.Setup(c => c.CurrentUser).Returns(new ApplicationUser
        {
            Id = 1,
            UserName = "u",
            FirstName = "A",
            LastName = "B",
            DateFormat = format
        });

        return new DateFormatService(_currentUserMock.Object);
    }

    [Fact]
    public void IsJalali_True_WhenUserPrefersJalali()
    {
        var service = CreateService(DateFormatType.Jalali);
        Assert.True(service.IsJalali);
    }

    [Fact]
    public void IsJalali_False_WhenUserPrefersGregorian()
    {
        var service = CreateService(DateFormatType.Gregorian);
        Assert.False(service.IsJalali);
    }

    [Fact]
    public void ToDisplayString_Jalali_FormatsPersianCalendar()
    {
        var service = CreateService(DateFormatType.Jalali);
        // 2024-03-20 is 1403-01-01 in the Persian calendar
        var date = new DateTime(2024, 3, 20);

        var result = service.ToDisplayString(date);

        Assert.Equal("1403/01/01", result);
    }

    [Fact]
    public void ToDisplayString_Gregorian_UsesSlashFormat()
    {
        var service = CreateService(DateFormatType.Gregorian);
        var date = new DateTime(2024, 3, 20);

        var result = service.ToDisplayString(date);

        Assert.Equal("2024/03/20", result);
    }

    [Fact]
    public void ToDisplayString_WithTime_AppendsTime()
    {
        var service = CreateService(DateFormatType.Gregorian);
        var date = new DateTime(2024, 3, 20, 14, 5, 0);

        var result = service.ToDisplayString(date, includeTime: true);

        Assert.Equal("2024/03/20 14:05", result);
    }

    [Fact]
    public void ToDisplayString_Null_ReturnsDash()
    {
        var service = CreateService(DateFormatType.Gregorian);

        var result = service.ToDisplayString(null);

        Assert.Equal("-", result);
    }

    [Fact]
    public void ToShortDisplayString_Gregorian_FormatsMonthDay()
    {
        var service = CreateService(DateFormatType.Gregorian);
        var date = new DateTime(2024, 3, 20);

        var result = service.ToShortDisplayString(date);

        Assert.Equal("03/20", result);
    }

    [Fact]
    public void ToShortDisplayString_Jalali_FormatsMonthDay()
    {
        var service = CreateService(DateFormatType.Jalali);
        var date = new DateTime(2024, 3, 20); // 1403/01/01

        var result = service.ToShortDisplayString(date);

        Assert.Equal("01/01", result);
    }
}
