using System.Collections.Generic;

namespace Ray.Dtos
{
    public class FacebookMediaResponseDto
    {
        public Thumbnails thumbnails { get; set; }
        public string id { get; set; }
    }

    public class Datum
    {
        public bool is_preferred { get; set; }
        public string uri { get; set; }
        public string id { get; set; }
    }

    public class Thumbnails
    {
        public List<Datum> data { get; set; }
    }
}
