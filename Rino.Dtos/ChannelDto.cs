namespace Rino.Dtos
{
    public class ChannelDto : BaseDto
    {
        public virtual string Name { get; set; }

        public virtual int Order { get; set; }

        public virtual bool IsDeleted { get; set; }
        public MediaDto Media { get; set; }
    }
}
