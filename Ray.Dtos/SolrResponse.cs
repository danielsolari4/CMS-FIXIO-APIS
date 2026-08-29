using System.Collections.Generic;

namespace Ray.Dtos
{
    public class SolrResponse
    {
        public response response { get;set;}
    }

    public class response
    {
        public int numFound { get; set; }
        public int start { get; set; }
        public List<dynamic> docs { get; set; }
    }
}
