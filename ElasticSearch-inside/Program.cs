using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Nest;
using ES = ElasticsearchInside;

namespace ElasticSearch_inside
{
    class Program
    {
        public static void DefaultSettings()
        {
            var settings = new ConnectionSettings()
                .DefaultIndex("defaultindex");
            var resolver = new IndexNameResolver(settings);
            var index = resolver.Resolve<Person>();
            index.Should().Be("defaultindex");
        }
        public static void TestInstanceOne(ES.Elasticsearch elasticsearch)
        {
            var test = new NestedDocument()
            {
                MemberOne = "Test",
                MemberTwo = new List<NestedObject>() { new NestedObject() { M1 = "Value1", M2 = "Value2" }, new NestedObject() { M1 = "Value3", M2 = "Value4" } }
            };

            var test2 = new NestedDocument()
            {
                MemberOne = "Test2",
                MemberTwo = new List<NestedObject>() { new NestedObject() { M1 = "Value5", M2 = "Value6" }, new NestedObject() { M1 = "Value7", M2 = "Value8" } }
            };

            var client = new ElasticClient(new ConnectionSettings(elasticsearch.Url)
                .DefaultIndex("mytestindex"));

            var createIndexResponse = client.CreateIndex("mytestindex", descriptor => descriptor
                .Mappings(map => map
                    .Map<NestedDocument>(m => m.AutoMap())));

            var idx = client.Index(test);
            var idx2 = client.Index(test2);

            //var myjson = new {hello = "world"};

            if (!client.IndexExists("mytestindex").Exists)
            {
                client.CreateIndex("mytestindex");
            }
            //var indexResponse = client.Index(myjson, i => i
            //    .Index("mytestindex")
            //    .Type("resp")
            //    .Id(1)
            //    .Refresh());

            //client.Index(test, idx => idx
            //    .Index("mytestindex")
            //    .Type("resp")
            //    .Id(1)
            //    .Refresh());

            //var descriptor = new CreateIndexDescriptor("mytestindex").Mappings(ms => ms.Map<NestedDocument>(m => m.AutoMap()));


            //var descriptor = new CreateIndexDescriptor("mytestindex")
            //    .Mappings(ms => ms
            //        .Map<NestedDocument>(m => m
            //            .Properties(ps => ps
            //                .String(s => s
            //                   .Name(c => c.MemberOne))
            //                   .Nested<NestedObject>(o => o
            //                   .Name(c => c.MemberTwo)
            //                   .Properties(eps => eps
            //                       .String(s => s
            //                           .Name(e => e.M1)
            //                        )
            //                        .String(s => s
            //                           .Name(e => e.M2)))))));


            /*                var t2 = client.Index(descriptor)*/;

            Thread.Sleep(5000);

            var mapping = client.GetMapping(new GetMappingRequest(Types.All));

            //var result = client.Ping();
            //Console.WriteLine(result);
            //Console.WriteLine(elasitcsearch.Url);
            //var request = new GetRequest("mytestindex","resp","1");
            //var request2 = new GetRequest("mytestindex", "resp", "2");
            //var response = client.Get<dynamic>(request);

            //var response2 = client.Search<NestedDocument>(q =>
            //    q.Query(query => query
            //        .Nested(cr => cr
            //            .Path(p => p.MemberTwo)
            //            .Query(pol =>
            //                pol.Term(p => p.MemberTwo.First().M1, "Value1")))));


            //            .Term(c => c
            //.Name("named_query")
            //.Boost(1.1)
            //.Field(p => p.Description)
            //.Value("project description")

            //var response3 =
            //    client.Search<NestedDocument>(q => q.Query(t => t.Term(c => c.MemberTwo.First().M1, "Value1")));
            //var response4 =
            //    client.Search<NestedDocument>(q => q.Query(t => t.Term(c => c.Name("nested_query").Boost(1.1).Field(p => p.MemberOne).Value("Value2"))));

            var response5 =
                client.Search<NestedDocument>(
                    search =>
                        search.Query(
                            q => q.QueryString(qs => qs.Fields(f => f.Field(p => p.MemberOne)).Query("Test"))));

            var response6 =
                client.Search<NestedDocument>(
                    search =>
                        search.Query(
                            q => q.QueryString(qs => qs.Fields(f => f.Field(p => p.MemberTwo.Select(x => x.M1))).Query("Value1")))).Documents.First().MemberOne;

            var result = response5.Documents;
            var result6 = response6;
            Console.Read();
        }
        public static void StoreDifferentTypesInElasticSearchIndex(ES.Elasticsearch elasticsearch)
        {
            var client = new ElasticClient(new ConnectionSettings(elasticsearch.Url));

            var milton = new Person()
            {
                Age = 1000,
                FirstName = "Milton",
                LastName = "Cody",
                JobId = Guid.NewGuid(),
                PersonId = Guid.NewGuid(),
                Sex = 'M'
            };

            var indexResponse = client.CreateIndex("differenttypesindex", descriptor => descriptor
                .Mappings( map => map
                    .Map<Person>(m => m.AutoMap())
                    .Map<Company>(m=> m
                        .AutoMap()
                        .Properties(ps => ps
                            .Nested<Employee>(n => n
                                .Name(c => c.Employees))))      
                 ));

            var details = indexResponse.CallDetails;
            var debugInformation = indexResponse.DebugInformation;
            var serverError = indexResponse.ServerError;
            

            var expected = new
                {
                    mappings = new
                    {
                        person = new
                        {
                            properties = new
                            {
                                personid = new
                                {
                                    type = "string"
                                },
                                jobid = new
                                {
                                    type = "string"
                                },
                                fristname = new
                                {
                                    type = "string"
                                },
                                lastname = new
                                {
                                    type = "string"
                                },
                                age = new
                                {
                                    type = "long"
                                },
                                sex = new
                                {
                                    type = "string"
                                }
                            }
                        },
                        company = new 
                        {
                            properties = new
                            {
                                employees = new
                                {
                                    type = "nested"
                                },
                                name = new
                                {
                                    type = "string"
                                }
                            }
                        },
                        employee = new
                        {
                            properties = new
                            {
                                birthday = new
                                {
                                    type = "date"
                                },
                                employees = new
                                {
                                    properties = new { },
                                    type = "object"
                                },
                                firstName = new
                                {
                                    type = "string"
                                },
                                hours = new
                                {
                                    type = "long"
                                },
                                isManager = new
                                {
                                    type = "boolean"
                                },
                                lastName = new
                                {
                                    type = "string"
                                },
                                salary = new
                                {
                                    type = "integer"
                                }
                            }
                        }
                    }
                };

            var response = client.LowLevel.IndicesGetMapping<dynamic>("_all","_all");

            var personMap = client.GetMapping<Person>();
            var companyMap = client.GetMapping<Company>();
            var employeeMap = client.GetMapping<Employee>(); // When a type is nested it does not define it's own document type.
            var mp = personMap.Mapping.Properties; // initially when you index data the types are not defined is this because there is no data.


            var response1 = client.Index<Person>(milton, idx => idx 
                .Index("differenttypesindex")
                .Type("person")
                .Id(1)
                .Refresh());


            //client.Index(test, idx => idx
            //    .Index("mytestindex")
            //    .Type("resp")
            //    .Id(1)
            //    .Refresh())
        }



        static void Main(string[] args)
        {
            using (var elasticsearch = new ElasticsearchInside.Elasticsearch(c => c.EnableLogging().LogTo(Console.WriteLine)))
            {
                StoreDifferentTypesInElasticSearchIndex(elasticsearch);
            }
        }
    }

    [ElasticsearchType(Name = "nesteddocument")]
    public class NestedDocument
    {
        [String(Name = "memberone")]
        public string MemberOne { get; set; }
        [Object(Name = "membertwo")]
        public List<NestedObject> MemberTwo { get; set; }
    }

    [ElasticsearchType(Name = "nestedobject")]
    public class NestedObject
    {
        [String(Name = "m1")]
        public string M1 { get; set; }
        [String(Name = "m2")]
        public string M2 { get; set; }
    }

    public class Company
    {
        public string Name { get; set; }
        public List<Employee> Employees { get; set; } 
    }

    public class Employee
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public decimal Salary { get; set; }
        public DateTime Birthday { get; set; }
        public bool IsManager { get; set; }
        public List<Employee> Employees { get; set; } 
        public TimeSpan Hours { get; set; }
    }




    public class Person
    {
        public Guid PersonId { get; set; }
        public Guid JobId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public char Sex { get; set; } 
    }

    public class Job
    {
        public Guid JobId { get; set; }
        public string Title { get; set; }
        public decimal Salary { get; set; }
    }
}
