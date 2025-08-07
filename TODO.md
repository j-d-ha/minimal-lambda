# TODO List

## Source Generator - MapHandlerIncrementalGenerator

### Completed (marked with x)
- ✅ ~~build out lambda args parser~~
- ✅ ~~add using imports for all types~~
- ✅ ~~add constructor parameters for all injected types~~
- ✅ ~~build out func type~~
- ✅ ~~build out lambda handler arguments~~
- ✅ ~~build out lambda invocation arguments~~
- ✅ ~~handle default values for parameters -> not needed~~
- ✅ ~~look into handling namespace vs type -> using `global::`~~
- ✅ ~~add code to handle injecting ILambdaSerializer~~
- ✅ ~~validate that nullable types work as expected and body~~

### Outstanding Items
- [ ] Add guards around number of arguments with request attributes and number of arguments with ILambdaContext
- [ ] Look into handling duplicate field names
- [ ] Update to handle situations where serializer is not needed
- [ ] Look into adding code to fail fast for DI stuff at startup
- [ ] Look into adding support for dependencies inside of an object like minimal APIs have
- [ ] Handle IServiceScope and IServiceProvider as arguments
