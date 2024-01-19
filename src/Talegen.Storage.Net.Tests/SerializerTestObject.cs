namespace Talegen.Storage.Net.Tests
{
    /// <summary>
    /// This class is a test object used for testing serializer logic.
    /// </summary>
    public class SerializerTestObject
    {
        /// <summary>
        /// Gets a default instance of this object.
        /// </summary>
        public static SerializerTestObject Default => new SerializerTestObject() { Name = "test" };

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }
    }
}
