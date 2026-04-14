using System;

public class BanDetails
{
	public string OriginalName;

	public string Id;

	public long Expires;

	public string Reason;

	public string Issuer;

	public long IssuanceTime;

	public override string ToString()
	{
		return string.Format("{0};{1};{2};{3};{4};{5}", OriginalName.Replace(";", ":"), Id.Replace(";", ":"), Convert.ToString(Expires), Reason.Replace(";", ":"), Issuer.Replace(";", ":"), Convert.ToString(IssuanceTime));
	}
}
