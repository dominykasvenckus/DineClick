"use client";

import Footer from "@/app/components/footer";
import RestaurantManagerHeader from "@/app/components/restaurant-manager-header";
import { getAccessToken, getDecodedAccessToken } from "@/app/jwt-utils";
import { Error, Restaurant } from "@/app/types";
import { useRouter } from "next/navigation";
import { ChangeEvent, FormEvent, ReactNode, useEffect, useState } from "react";

export default function Restaurant({
  params,
}: {
  params: { cityId: number; restaurantId: number };
}) {
  const [content, setContent] = useState<null | ReactNode>(null);
  const [formData, setFormData] = useState({
    name: "",
    description: "",
    streetAddress: "",
    websiteUrl: "",
  });
  const [successMessage, setSuccessMessage] = useState("");
  const [errorMessage, setErrorMessage] = useState("");
  const router = useRouter();

  const handleChange = (
    e: ChangeEvent<HTMLInputElement> | ChangeEvent<HTMLTextAreaElement>
  ) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
  };

  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    const accessToken = await getAccessToken();
    if (!accessToken) {
      router.push("/login");
      return;
    }

    setSuccessMessage("");
    setErrorMessage("");

    const response = await fetch(
      `/api/v1/cities/${params.cityId}/restaurants/${params.restaurantId}`,
      {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${accessToken}`,
        },
        body: JSON.stringify(formData),
      }
    );

    if (response.ok) {
      const restaurant: Restaurant = await response.json();
      setFormData({
        name: restaurant.name,
        description: restaurant.description,
        streetAddress: restaurant.streetAddress,
        websiteUrl: restaurant.websiteUrl,
      });
      setSuccessMessage("Changes saved successfully.");
    }

    if (response.status === 422) {
      const errorData: Error | Error[] = await response.json();
      const errorMessage = Array.isArray(errorData)
        ? errorData.map((error) => error.error).join("\n")
        : errorData.error;
      setErrorMessage(errorMessage);
    }
  };

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
        case "Admin":
          router.push("/login");
          return;
        case "RestaurantManager":
          setContent(
            <>
              <RestaurantManagerHeader />
              <div className="w-4/5 p-6 m-auto bg-white dark:bg-gray-700 rounded-md shadow-md ring-2 ring-gray-800/50 dark:ring-gray-300/50 md:max-w-lg my-16">
                <h1 className="text-xl font-semibold text-center text-gray-700 dark:text-white">
                  Restaurant
                </h1>
                {successMessage && (
                  <div className="text-green-600 text-sm md:text-base mt-2">
                    {successMessage}
                  </div>
                )}
                {errorMessage && (
                  <div className="text-red-500 text-sm md:text-base mt-2 whitespace-pre-line">
                    {errorMessage}
                  </div>
                )}
                <form method="post" onSubmit={handleSubmit} autoComplete="off">
                  <div>
                    <label
                      htmlFor="name"
                      className="label text-sm md:text-base"
                    >
                      Name
                    </label>
                    <input
                      id="name"
                      type="text"
                      name="name"
                      placeholder="Enter name"
                      defaultValue={formData.name}
                      onChange={handleChange}
                      className="w-full input input-bordered text-sm md:text-base"
                    />
                  </div>
                  <div className="mt-2">
                    <label
                      htmlFor="description"
                      className="label text-sm md:text-base"
                    >
                      Description
                    </label>
                    <textarea
                      id="description"
                      name="description"
                      placeholder="Enter description"
                      rows={4}
                      defaultValue={formData.description}
                      onChange={handleChange}
                      className="w-full textarea textarea-bordered text-sm md:text-base"
                    />
                  </div>
                  <div className="mt-2">
                    <label
                      htmlFor="streetAddress"
                      className="label text-sm md:text-base"
                    >
                      Street address
                    </label>
                    <input
                      id="streetAddress"
                      type="text"
                      name="streetAddress"
                      placeholder="Enter street address"
                      defaultValue={formData.streetAddress}
                      onChange={handleChange}
                      className="w-full input input-bordered text-sm md:text-base"
                    />
                  </div>
                  <div className="mt-2">
                    <label
                      htmlFor="websiteUrl"
                      className="label text-sm md:text-base"
                    >
                      Website URL
                    </label>
                    <input
                      id="websiteUrl"
                      type="text"
                      name="websiteUrl"
                      placeholder="Enter website URL"
                      defaultValue={formData.websiteUrl}
                      onChange={handleChange}
                      className="w-full input input-bordered text-sm md:text-base"
                    />
                  </div>
                  <div className="mt-8">
                    <button type="submit" className="btn btn-block">
                      Save
                    </button>
                  </div>
                </form>
              </div>
              <Footer />
            </>
          );
          break;
      }
    })();
  }, [formData, successMessage, errorMessage]);

  useEffect(() => {
    (async () => {
      const accessToken = await getAccessToken();
      const decodedAccessToken = await getDecodedAccessToken();
      if (!accessToken || !decodedAccessToken) {
        router.push("/login");
        return;
      }

      const response = await fetch(
        `/api/v1/cities/${params.cityId}/restaurants/${params.restaurantId}`,
        {
          method: "GET",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${accessToken}`,
          },
        }
      );

      if (response.ok) {
        const restaurant: Restaurant = await response.json();
        setFormData({
          name: restaurant.name,
          description: restaurant.description,
          streetAddress: restaurant.streetAddress,
          websiteUrl: restaurant.websiteUrl,
        });
      }
    })();
  }, []);

  return content;
}
