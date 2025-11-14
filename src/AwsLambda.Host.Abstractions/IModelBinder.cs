using System.Text.Json;

namespace AwsLambda.Host;

public interface IModelBinder
{
    void BindModel(JsonSerializerOptions options);

    void UnbindModel(JsonSerializerOptions options);
}
