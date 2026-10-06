namespace Whizible26.Domain.Entity
{
    /// <summary>
    /// Row shape returned by usp_ValidateActiveLogin (maps via GetAsyncSP / AutoMapper).
    /// </summary>
    public class ValidateActiveLoginEntity
    {
        public int LoginID { get; set; }
        public string? LoginName { get; set; }
        public string? Password { get; set; }
        public Guid? SecurityStamp { get; set; }
        public string? UserName { get; set; }
        /// <summary>Display name column spelling in database.</summary>
        public string? DispalyName { get; set; }
    }
}
