public interface ICentralAuth
{
	void TokenGenerated(string token);

	void RequestBadge(string token);

	void Fail();

	CharacterClassManager GetCcm();

	void Ok(string steamId, string nickname, string ban, string steamban, string server, bool bypass, bool DNT);

	void FailToken(string reason);
}
