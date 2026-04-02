let uploadFn = null;

async function loadAppwrite() {
  if (!uploadFn) {
    const module =
      await import("https://cdn.jsdelivr.net/npm/appwrite@13.0.0/+esm");

    const appwriteConfig = {
      projectId: "69c3825000060101ae39",
      appName: "test-proj",
      endPoint: "https://sgp.cloud.appwrite.io/v1",
      bucketId: "69ce3a7d001cfa3d492b",
    };
    const client = new module.Client()
      .setEndpoint(appwriteConfig.endPoint)
      .setProject(appwriteConfig.projectId);

    const storage = new module.Storage(client);

    uploadFn = async (file) => {
      const res = await storage.createFile(
        appwriteConfig.bucketId, // ✅ correct
        module.ID.unique(),

        file,
      );

      return `${appwriteConfig.endPoint}/storage/buckets/${appwriteConfig.bucketId}/files/${res.$id}/view?project=${appwriteConfig.projectId}`;
    };
  }
}
