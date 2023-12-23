"use client";

import { useRouter } from "next/navigation";
import { useEffect } from "react";
import { getDecodedAccessToken } from "./jwt-utils";

export default function Home() {
  const router = useRouter();

  useEffect(() => {
    (async () => {
      let decodedAccessToken = await getDecodedAccessToken();
      if (!decodedAccessToken) {
        router.push("/login");
        return;
      }

      const role =
        decodedAccessToken[
          "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
        ];

      switch (role) {
        case "RegisteredUser":
        case "RestaurantManager":
          router.push("/restaurants");
          return;
        case "Admin":
          router.push("/users");
          return;
      }
    })();
  }, []);

  return null;
}
