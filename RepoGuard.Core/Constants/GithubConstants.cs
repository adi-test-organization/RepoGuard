namespace RepoGuard.Core.Constants;

public static class GithubConstants
{
    public static class Headers
    {
        public const string Event = "X-GitHub-Event";
        public const string Signature = "X-Hub-Signature-256";
    }

    public static class Events
    {
        public const string Push = "push";
        public const string Repository = "repository";
        public const string Team = "team";
    }

    public static class Actions
    {
        public const string Created = "created";
        public const string Deleted = "deleted";
    }

    public static class JsonProperties
    {
        public const string Action = "action";
        public const string Repository = "repository";
        public const string Team = "team";
        public const string Sender = "sender";
        public const string Pusher = "pusher";
        public const string Name = "name";
        public const string FullName = "full_name";
        public const string Login = "login";
        public const string CreatedAt = "created_at";
        public const string PushedAt = "pushed_at";
        public const string UpdatedAt = "updated_at";
        public const string Id = "id";
        public const string HeadCommit = "head_commit";
        public const string Timestamp = "timestamp";
    }
}