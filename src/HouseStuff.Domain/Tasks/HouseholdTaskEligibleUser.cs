namespace HouseStuff.Domain.Tasks;

public sealed class HouseholdTaskEligibleUser
{
    private HouseholdTaskEligibleUser() { }

    internal HouseholdTaskEligibleUser(Guid householdTaskId, Guid residenceId, string userId)
    {
        HouseholdTaskId = householdTaskId;
        ResidenceId = residenceId;
        UserId = userId;
    }

    public Guid HouseholdTaskId { get; private set; }
    public Guid ResidenceId { get; private set; }
    public string UserId { get; private set; } = string.Empty;
}
