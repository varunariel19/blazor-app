using System.Numerics;
using Google.Protobuf.Collections;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace SolveIt.Services
{
    public class VectorPayloadType
    {
        public string QuestionId { get; set; } = string.Empty;
        public string Question { get; set; } = string.Empty;
    }
    public class VectorService(QdrantClient client)
    {

        private readonly QdrantClient _client = client;

        public async Task InsertVectorAsync(string collectionName , float[] embeddings , MapField<string , Value> payload)
        {
            var point = new PointStruct
            {
                Id = Guid.NewGuid(),
                Vectors = embeddings,
            };

            point.Payload.Add(payload);

            await _client.UpsertAsync(collectionName, [point]);

        }

        public async Task<List<Guid>> SearchVectorAsync(string collectionName, float[] vector) 
        {
            var searchResults = await _client.SearchAsync(
                     collectionName: collectionName,
                     vector: vector!,
                     limit: 10
                 );
          
            var questionIds = searchResults
                .Select(r => Guid.Parse(r.Payload["questionId"].StringValue))
                .ToList();

            return questionIds;


        }

    }
}
