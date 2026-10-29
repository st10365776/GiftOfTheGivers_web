using System.ComponentModel.DataAnnotations;
using GiftOfTheGivers_web.Models.ViewModels;
using Xunit;

namespace GiftOfTheGivers_web.Tests;

public class DonationViewModelTests
{
    [Fact]
    public void ValidDonationAmountPassesValidation()
    {
        var model = new DonationViewModel { Amount = 250 };

        var results = Validate(model);

        Assert.Empty(results);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(100000001)]
    public void AmountOutsideAllowedRangeFailsValidation(decimal amount)
    {
        var model = new DonationViewModel { Amount = amount };

        var results = Validate(model);

        Assert.Contains(results, result => result.MemberNames.Contains(nameof(model.Amount)));
    }

    [Fact]
    public void MissingDonationDetailsFailValidation()
    {
        var model = new DonationViewModel
        {
            Amount = 250,
            Currency = null!,
            DonationType = null!,
            PaymentMethod = null!
        };

        var results = Validate(model);

        Assert.Contains(results, result => result.MemberNames.Contains(nameof(model.Currency)));
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(model.DonationType)));
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(model.PaymentMethod)));
    }

    private static IList<ValidationResult> Validate(DonationViewModel model)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(
            model,
            new ValidationContext(model),
            results,
            validateAllProperties: true);

        return results;
    }
}
