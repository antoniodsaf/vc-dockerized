import { graphqlClient } from "../../client";
import type { CategoryConnection } from "../../types";
import getMyCategoriesQueryDocument from "./customNavbar.graphql";

export async function getMyCategories(payload: {
  storeId: string;
}): Promise<{ categories: { items: CategoryConnection["items"] } }> {
  const { data } = await graphqlClient.query({
    query: getMyCategoriesQueryDocument,
    variables: payload,
  });

  return data;
}
