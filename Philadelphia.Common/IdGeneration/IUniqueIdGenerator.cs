namespace Philadelphia.Common {
    // REVIEW: rename file to UniqueIdGenerator
    // Question: maybe IUniqueIdGeneratorImplementation and DefaultUniqueIdGenerator be nested within UniqueIdGenerator?
	public static class UniqueIdGenerator {
	    private static IUniqueIdGeneratorImplementation _impl = new DefaultUniqueIdGenerator();

	    public static int Generate() {
		    return _impl.Generate();
		}

	    public static string GenerateAsString() {
	        return Generate().ToString();
	    }

        public static void SetImplementation(IUniqueIdGeneratorImplementation impl) {
            _impl = impl;
        }
	}
}
