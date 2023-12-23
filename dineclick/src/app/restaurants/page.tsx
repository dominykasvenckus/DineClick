"use client";

import { ReactNode, useEffect, useState } from "react";
import { City, Restaurant } from "../types";
import { useRouter } from "next/navigation";
import RegisteredUserHeader from "../components/registered-user-header";
import RestaurantManagerHeader from "../components/restaurant-manager-header";
import AdminHeader from "../components/admin-header";
import { getAccessToken, getDecodedAccessToken } from "../jwt-utils";
import Link from "next/link";
import Footer from "../components/footer";

export default function Restaurants() {
  const [content, setContent] = useState<null | ReactNode>(null);
  const [cities, setCities] = useState<null | City[]>(null);
  const [selectedCityId, setSelectedCityId] = useState<string>("");
  const [restaurants, setRestaurants] = useState<null | Restaurant[]>(null);
  const router = useRouter();

  const renderHeader = (role: string) => {
    switch (role) {
      case "RegisteredUser":
        return <RegisteredUserHeader />;
      case "RestaurantManager":
        return <RestaurantManagerHeader />;
      case "Admin":
        return <AdminHeader />;
      default:
        return null;
    }
  };

  const deleteRestaurant = async (restaurantId: number) => {
    const accessToken = await getAccessToken();
    if (!accessToken) {
      router.push("/login");
      return;
    }

    const response = await fetch(
      `/api/v1/cities/${selectedCityId}/restaurants/${restaurantId}`,
      {
        method: "DELETE",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${accessToken}`,
        },
      }
    );

    if (response.status === 204) {
      setRestaurants((prevRestaurants) => {
        if (prevRestaurants) {
          return prevRestaurants.filter(
            (restaurant) => restaurant.restaurantId !== restaurantId
          );
        }
        return prevRestaurants;
      });
    }
  };

  const renderButtons = (role: string, restaurantId: number) => {
    switch (role) {
      case "RegisteredUser":
        return (
          <div className="flex flex-wrap gap-3">
            <Link
              href={`/cities/${selectedCityId}/restaurants/${restaurantId}/reservations/create`}
              className="flex-initial btn btn-neutral"
            >
              Reserve
            </Link>
            <Link
              href={`/cities/${selectedCityId}/restaurants/${restaurantId}/reservations`}
              className="flex-initial btn btn-neutral"
            >
              View reservations
            </Link>
          </div>
        );
      case "RestaurantManager":
        return (
          <div className="flex flex-wrap gap-3">
            <Link
              href={`/cities/${selectedCityId}/restaurants/${restaurantId}/reservations`}
              className="flex-initial btn btn-neutral"
            >
              View reservations
            </Link>
            <Link
              href={`/cities/${selectedCityId}/restaurants/${restaurantId}`}
              className="flex-initial btn btn-neutral"
            >
              Edit restaurant
            </Link>
            <button
              className="flex-initial btn btn-neutral"
              onClick={() => deleteRestaurant(restaurantId)}
            >
              Delete restaurant
            </button>
          </div>
        );
      case "Admin":
        return (
          <button
            className="flex-initial btn btn-neutral"
            onClick={() => deleteRestaurant(restaurantId)}
          >
            Delete restaurant
          </button>
        );
      default:
        return null;
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
        case "RestaurantManager":
        case "Admin":
          setContent(
            <>
              {renderHeader(role)}
              <div className="p-5">
                <div className="flex items-center justify-center mb-5">
                  <select
                    id="cities"
                    className="select select-bordered border-neutral dark:border-white w-full max-w-md text-sm md:text-base"
                    defaultValue={""}
                    onChange={(e) => setSelectedCityId(e.target.value)}
                  >
                    <option hidden value="">
                      Select city
                    </option>
                    {cities?.map((city) => (
                      <option key={city.cityId} value={city.cityId}>
                        {city.name}
                      </option>
                    ))}
                  </select>
                  {role === "RestaurantManager" && (
                    <Link
                      href="/restaurants/create"
                      className="flex-initial btn btn-neutral ml-2"
                    >
                      Create restaurant
                    </Link>
                  )}
                </div>
                {restaurants?.length === 0 && (
                  <div className="text-sm md:text-base text-center">
                    No restaurants were found in this city
                  </div>
                )}
                {restaurants?.map((restaurant) => (
                  <div
                    key={restaurant.restaurantId}
                    className="collapse collapse-arrow bg-accent max-w-6xl mb-2 m-auto"
                  >
                    <input
                      id={`checkbox-${restaurant.restaurantId}`}
                      type="checkbox"
                    />
                    <div className="collapse-title text-sm md:text-base font-medium">
                      {restaurant.name}
                      <div className="whitespace-nowrap">
                        <svg
                          xmlns="http://www.w3.org/2000/svg"
                          fill="none"
                          viewBox="0 0 24 24"
                          strokeWidth="1.5"
                          stroke="currentColor"
                          className="w-5 h-5 md:w-6 md:h-6 inline-block align-top mr-1"
                        >
                          <path
                            strokeLinecap="round"
                            strokeLinejoin="round"
                            d="M15 10.5a3 3 0 11-6 0 3 3 0 016 0z"
                          />
                          <path
                            strokeLinecap="round"
                            strokeLinejoin="round"
                            d="M19.5 10.5c0 7.142-7.5 11.25-7.5 11.25S4.5 17.642 4.5 10.5a7.5 7.5 0 1115 0z"
                          />
                        </svg>
                        {restaurant.streetAddress}
                      </div>
                    </div>
                    <div className="collapse-content text-sm md:text-base">
                      <div className="mb-3">
                        {restaurant.description}
                        <div className="whitespace-nowrap mt-2">
                          <svg
                            xmlns="http://www.w3.org/2000/svg"
                            fill="none"
                            viewBox="0 0 24 24"
                            strokeWidth="1.5"
                            stroke="currentColor"
                            className="w-5 h-5 md:w-6 md:h-6 inline-block align-top mr-1"
                          >
                            <path
                              strokeLinecap="round"
                              strokeLinejoin="round"
                              d="M12 21a9.004 9.004 0 008.716-6.747M12 21a9.004 9.004 0 01-8.716-6.747M12 21c2.485 0 4.5-4.03 4.5-9S14.485 3 12 3m0 18c-2.485 0-4.5-4.03-4.5-9S9.515 3 12 3m0 0a8.997 8.997 0 017.843 4.582M12 3a8.997 8.997 0 00-7.843 4.582m15.686 0A11.953 11.953 0 0112 10.5c-2.998 0-5.74-1.1-7.843-2.918m15.686 0A8.959 8.959 0 0121 12c0 .778-.099 1.533-.284 2.253m0 0A17.919 17.919 0 0112 16.5c-3.162 0-6.133-.815-8.716-2.247m0 0A9.015 9.015 0 013 12c0-1.605.42-3.113 1.157-4.418"
                            />
                          </svg>
                          <a href={restaurant.websiteUrl}>
                            {restaurant.websiteUrl}
                          </a>
                        </div>
                      </div>
                      {renderButtons(role, restaurant.restaurantId)}
                    </div>
                  </div>
                ))}
              </div>
              <Footer />
            </>
          );
          break;
      }
    })();
  }, [cities, restaurants]);

  useEffect(() => {
    (async () => {
      const accessToken = await getAccessToken();
      if (!accessToken) {
        router.push("/login");
        return;
      }

      const response = await fetch("/api/v1/cities", {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${accessToken}`,
        },
      });

      if (response.ok) {
        const cities: City[] = await response.json();
        cities.sort((city1, city2) => city1.name.localeCompare(city2.name));
        setCities(cities);
      }
    })();
  }, []);

  useEffect(() => {
    (async () => {
      const accessToken = await getAccessToken();
      if (!accessToken) {
        router.push("/login");
        return;
      }

      if (!selectedCityId) {
        return;
      }

      const response = await fetch(
        `/api/v1/cities/${selectedCityId}/restaurants`,
        {
          method: "GET",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${accessToken}`,
          },
        }
      );

      if (response.ok) {
        const restaurants: Restaurant[] = await response.json();
        setRestaurants(restaurants);
      }
    })();
  }, [selectedCityId]);

  return content;
}
