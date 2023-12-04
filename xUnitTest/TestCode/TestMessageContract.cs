using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xUnitTest.TestCode
{
    internal class TestMessageContract
    {
        public string Name { get; set; }
        public string Data { get; set; }
    }

    internal static class TestMessageHandlers
    {
        public static async Task TestMessageContractProcessor(TestMessageContract message)
        {
            // Your message processing logic here
            Console.WriteLine($"Processing message: {message}");
            //put a break point and drag to simulate exception
            if( message == null )
                throw new ArgumentNullException( "message" );
        }
    }
}
