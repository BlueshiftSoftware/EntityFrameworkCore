using Blueshift.EntityFrameworkCore.MongoDB.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

namespace Blueshift.EntityFrameworkCore.MongoDB.Tests.Metadata
{
    public class MongoDbModelAnnotationsTests
    {
        [Fact]
        public void Database_name_null_by_default()
        {
            var mongoDbModelAnnotations = new MongoDbModelAnnotations(new Model());
            Assert.Null(mongoDbModelAnnotations.Database);
        }

        [Fact]
        public void Can_write_database_name()
        {
            var mongoDbModelAnnotations = new MongoDbModelAnnotations(new Model()) { Database = "test" };
            Assert.Equal(expected: "test", actual: mongoDbModelAnnotations.Database);
        }
    }
}