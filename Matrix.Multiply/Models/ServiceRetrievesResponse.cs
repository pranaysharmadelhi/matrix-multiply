using System;
namespace Matrix.Multiply.Models
{
    public class ServiceRetrievesResponse
    {
        public int[] Value { get; set; }
        public string Cause { get; set; }
        public Boolean Success { get; set; }
    }
}
