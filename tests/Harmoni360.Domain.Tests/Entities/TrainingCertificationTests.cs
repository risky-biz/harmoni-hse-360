using FluentAssertions;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Enums;
using Harmoni360.Domain.Events;
using Xunit;

namespace Harmoni360.Domain.Tests.Entities;

public class TrainingCertificationTests
{
    [Fact]
    public void Create_ShouldCreateCertificationWithValidData()
    {
        // Arrange
        var trainingId = 1;
        var participantId = 2;
        var issuedBy = 3;
        var validityMonths = 12;
        var competencyAchieved = "ISO 45001 Safety Management";

        // Act
        var certification = TrainingCertification.Create(
            trainingId: trainingId,
            participantId: participantId,
            issuedBy: issuedBy,
            validityMonths: validityMonths,
            competencyAchieved: competencyAchieved
        );

        // Assert
        certification.Should().NotBeNull();
        certification.TrainingId.Should().Be(trainingId);
        certification.ParticipantId.Should().Be(participantId);
        certification.IssuedBy.Should().Be(issuedBy);
        certification.ValidityMonths.Should().Be(validityMonths);
        certification.CompetencyAchieved.Should().Be(competencyAchieved);
        certification.Status.Should().Be(CertificationStatus.Valid);
        certification.IssuedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        certification.ExpiryDate.Should().BeCloseTo(DateTime.UtcNow.AddMonths(validityMonths), TimeSpan.FromSeconds(1));
        certification.CertificationNumber.Should().NotBeNullOrEmpty();
        certification.CertificationNumber.Should().StartWith("CERT-");

        // Domain event should be raised
        certification.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<CertificationIssuedEvent>();
        
        var domainEvent = (CertificationIssuedEvent)certification.DomainEvents.First();
        domainEvent.TrainingId.Should().Be(trainingId);
        domainEvent.ParticipantId.Should().Be(participantId);
        domainEvent.CertificationNumber.Should().Be(certification.CertificationNumber);
        domainEvent.ExpiryDate.Should().Be(certification.ExpiryDate);
    }

    [Fact]
    public void Create_WithZeroValidityMonths_ShouldCreatePermanentCertification()
    {
        // Arrange & Act
        var certification = TrainingCertification.Create(
            trainingId: 1,
            participantId: 2,
            issuedBy: 3,
            validityMonths: 0, // Permanent
            competencyAchieved: "Basic Safety Awareness"
        );

        // Assert
        certification.ValidityMonths.Should().Be(0);
        certification.ExpiryDate.Should().BeNull();
        certification.Status.Should().Be(CertificationStatus.Valid);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-12)]
    public void Create_WithNegativeValidityMonths_ShouldThrowArgumentException(int invalidMonths)
    {
        // Act & Assert
        var act = () => TrainingCertification.Create(
            trainingId: 1,
            participantId: 2,
            issuedBy: 3,
            validityMonths: invalidMonths,
            competencyAchieved: "Test Competency"
        );

        act.Should().Throw<ArgumentException>()
            .WithMessage("*validity months*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidCompetency_ShouldThrowArgumentException(string invalidCompetency)
    {
        // Act & Assert
        var act = () => TrainingCertification.Create(
            trainingId: 1,
            participantId: 2,
            issuedBy: 3,
            validityMonths: 12,
            competencyAchieved: invalidCompetency
        );

        act.Should().Throw<ArgumentException>()
            .WithMessage("*competency*");
    }

    [Fact]
    public void Renew_ShouldExtendExpiryDateAndResetStatus()
    {
        // Arrange
        var certification = CreateValidCertification();
        var renewedBy = 5;
        var newValidityMonths = 24;

        // Act
        certification.Renew(renewedBy, newValidityMonths);

        // Assert
        certification.Status.Should().Be(CertificationStatus.Valid);
        certification.RenewedBy.Should().Be(renewedBy);
        certification.RenewedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        certification.ExpiryDate.Should().BeCloseTo(DateTime.UtcNow.AddMonths(newValidityMonths), TimeSpan.FromSeconds(1));
        certification.ValidityMonths.Should().Be(newValidityMonths);
        certification.ModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Renew_PermanentCertification_ShouldRemainPermanent()
    {
        // Arrange
        var certification = TrainingCertification.Create(
            trainingId: 1,
            participantId: 2,
            issuedBy: 3,
            validityMonths: 0, // Permanent
            competencyAchieved: "Test Competency"
        );

        // Act
        certification.Renew(5, 0); // Keep permanent

        // Assert
        certification.ExpiryDate.Should().BeNull();
        certification.ValidityMonths.Should().Be(0);
        certification.Status.Should().Be(CertificationStatus.Valid);
    }

    [Fact]
    public void Revoke_ShouldUpdateStatusAndSetRevocationInfo()
    {
        // Arrange
        var certification = CreateValidCertification();
        var revokedBy = 5;
        var reason = "Misconduct discovered";

        // Act
        certification.Revoke(revokedBy, reason);

        // Assert
        certification.Status.Should().Be(CertificationStatus.Revoked);
        certification.RevokedBy.Should().Be(revokedBy);
        certification.RevokedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        certification.RevocationReason.Should().Be(reason);
        certification.ModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Revoke_WithInvalidReason_ShouldThrowArgumentException(string invalidReason)
    {
        // Arrange
        var certification = CreateValidCertification();

        // Act & Assert
        var act = () => certification.Revoke(5, invalidReason);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*reason*");
    }

    [Fact]
    public void Suspend_ShouldUpdateStatusAndSetSuspensionInfo()
    {
        // Arrange
        var certification = CreateValidCertification();
        var suspendedBy = 5;
        var reason = "Under investigation";
        var suspensionEndDate = DateTime.UtcNow.AddDays(30);

        // Act
        certification.Suspend(suspendedBy, reason, suspensionEndDate);

        // Assert
        certification.Status.Should().Be(CertificationStatus.Suspended);
        certification.SuspendedBy.Should().Be(suspendedBy);
        certification.SuspendedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        certification.SuspensionReason.Should().Be(reason);
        certification.SuspensionEndDate.Should().Be(suspensionEndDate);
        certification.ModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Suspend_WithPastEndDate_ShouldThrowArgumentException()
    {
        // Arrange
        var certification = CreateValidCertification();
        var pastDate = DateTime.UtcNow.AddDays(-1);

        // Act & Assert
        var act = () => certification.Suspend(5, "Test reason", pastDate);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*future*");
    }

    [Fact]
    public void Reinstate_ShouldRestoreValidStatus()
    {
        // Arrange
        var certification = CreateValidCertification();
        certification.Suspend(5, "Test reason", DateTime.UtcNow.AddDays(30));
        var reinstatedBy = 6;

        // Act
        certification.Reinstate(reinstatedBy);

        // Assert
        certification.Status.Should().Be(CertificationStatus.Valid);
        certification.ReinstatedBy.Should().Be(reinstatedBy);
        certification.ReinstatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        certification.ModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Reinstate_WhenNotSuspended_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var certification = CreateValidCertification(); // Valid status

        // Act & Assert
        var act = () => certification.Reinstate(5);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*suspended*");
    }

    [Fact]
    public void UpdateCompetency_ShouldUpdateCompetencyAchieved()
    {
        // Arrange
        var certification = CreateValidCertification();
        var newCompetency = "Advanced Safety Management";

        // Act
        certification.UpdateCompetency(newCompetency);

        // Assert
        certification.CompetencyAchieved.Should().Be(newCompetency);
        certification.ModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void IsExpired_WhenPastExpiryDate_ShouldReturnTrue()
    {
        // Arrange
        var certification = CreateValidCertification();
        // Manually set expiry date to past (simulating expired certification)
        var pastDate = DateTime.UtcNow.AddDays(-1);
        certification.GetType().GetProperty("ExpiryDate")?.SetValue(certification, pastDate);

        // Act & Assert
        certification.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_WhenFutureExpiryDate_ShouldReturnFalse()
    {
        // Arrange
        var certification = CreateValidCertification();

        // Act & Assert
        certification.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_WhenPermanentCertification_ShouldReturnFalse()
    {
        // Arrange
        var certification = TrainingCertification.Create(
            trainingId: 1,
            participantId: 2,
            issuedBy: 3,
            validityMonths: 0, // Permanent
            competencyAchieved: "Test Competency"
        );

        // Act & Assert
        certification.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void IsExpiring_WhenWithin30Days_ShouldReturnTrue()
    {
        // Arrange
        var certification = CreateValidCertification();
        // Set expiry date to 15 days from now
        var soonExpiryDate = DateTime.UtcNow.AddDays(15);
        certification.GetType().GetProperty("ExpiryDate")?.SetValue(certification, soonExpiryDate);

        // Act & Assert
        certification.IsExpiring.Should().BeTrue();
    }

    [Fact]
    public void IsExpiring_WhenBeyond30Days_ShouldReturnFalse()
    {
        // Arrange
        var certification = CreateValidCertification(); // 12 months validity

        // Act & Assert
        certification.IsExpiring.Should().BeFalse();
    }

    [Fact]
    public void IsExpiring_WhenPermanentCertification_ShouldReturnFalse()
    {
        // Arrange
        var certification = TrainingCertification.Create(
            trainingId: 1,
            participantId: 2,
            issuedBy: 3,
            validityMonths: 0, // Permanent
            competencyAchieved: "Test Competency"
        );

        // Act & Assert
        certification.IsExpiring.Should().BeFalse();
    }

    [Fact]
    public void DaysUntilExpiry_WhenHasExpiryDate_ShouldReturnCorrectDays()
    {
        // Arrange
        var certification = CreateValidCertification();
        var expectedDays = (int)(certification.ExpiryDate!.Value.Date - DateTime.UtcNow.Date).TotalDays;

        // Act & Assert
        certification.DaysUntilExpiry.Should().Be(expectedDays);
    }

    [Fact]
    public void DaysUntilExpiry_WhenPermanentCertification_ShouldReturnNull()
    {
        // Arrange
        var certification = TrainingCertification.Create(
            trainingId: 1,
            participantId: 2,
            issuedBy: 3,
            validityMonths: 0, // Permanent
            competencyAchieved: "Test Competency"
        );

        // Act & Assert
        certification.DaysUntilExpiry.Should().BeNull();
    }

    [Fact]
    public void IsActive_WhenValidAndNotExpired_ShouldReturnTrue()
    {
        // Arrange
        var certification = CreateValidCertification();

        // Act & Assert
        certification.IsActive.Should().BeTrue();
    }

    [Fact]
    public void IsActive_WhenRevoked_ShouldReturnFalse()
    {
        // Arrange
        var certification = CreateValidCertification();
        certification.Revoke(5, "Test reason");

        // Act & Assert
        certification.IsActive.Should().BeFalse();
    }

    [Fact]
    public void IsActive_WhenSuspended_ShouldReturnFalse()
    {
        // Arrange
        var certification = CreateValidCertification();
        certification.Suspend(5, "Test reason", DateTime.UtcNow.AddDays(30));

        // Act & Assert
        certification.IsActive.Should().BeFalse();
    }

    [Fact]
    public void IsActive_WhenExpired_ShouldReturnFalse()
    {
        // Arrange
        var certification = CreateValidCertification();
        // Set expired status
        certification.GetType().GetProperty("Status")?.SetValue(certification, CertificationStatus.Expired);

        // Act & Assert
        certification.IsActive.Should().BeFalse();
    }

    private static TrainingCertification CreateValidCertification()
    {
        return TrainingCertification.Create(
            trainingId: 1,
            participantId: 2,
            issuedBy: 3,
            validityMonths: 12,
            competencyAchieved: "Basic Safety Management"
        );
    }
}