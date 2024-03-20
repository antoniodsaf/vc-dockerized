import { graphqlClient } from "../../client";
import getMyBalanceQueryDocument from "./custom.graphql";

export async function getMyBalance(payload: { storeId: string }): Promise<Record<string, Record<string, number>>> {
  const { data } = await graphqlClient.query({
    query: getMyBalanceQueryDocument,
    variables: payload,
  });

  return data;
}
