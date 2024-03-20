import { graphqlClient } from "../../client";
import cpqCOnfigQueryDocument from "./cpq.graphql";

export async function getCPQConfig(payload: { productLine: string }): Promise<{
  cpqConfigure: {
    loginSuccess: boolean;
    url: string;
  };
}> {
  console.log(payload);
  const { data } = await graphqlClient.query({
    query: cpqCOnfigQueryDocument,
    variables: payload,
  });

  return data;
}
