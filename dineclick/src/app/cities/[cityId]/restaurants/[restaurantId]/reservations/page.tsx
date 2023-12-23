"use client";

import Footer from "@/app/components/footer";
import RegisteredUserHeader from "@/app/components/registered-user-header";
import RestaurantManagerHeader from "@/app/components/restaurant-manager-header";
import { getAccessToken, getDecodedAccessToken } from "@/app/jwt-utils";
import { Reservation } from "@/app/types";
import { useRouter } from "next/navigation";
import { ReactNode, useEffect, useState } from "react";

export default function Reservations({
  params,
}: {
  params: { cityId: number; restaurantId: number };
}) {
  const [content, setContent] = useState<null | ReactNode>(null);
  const [reservations, setReservations] = useState<null | Reservation[]>(null);
  const router = useRouter();

  const renderHeader = (role: string) => {
    switch (role) {
      case "RegisteredUser":
        return <RegisteredUserHeader />;
      case "RestaurantManager":
        return <RestaurantManagerHeader />;
      default:
        return null;
    }
  };

  const getStatusName = (statusNumber: number) => {
    switch (statusNumber) {
      case 0:
        return "Pending";
      case 1:
        return "Confirmed";
      case 2:
        return "Canceled";
      default:
        return "Unknown status";
    }
  };

  const confirmReservation = async (reservation: Reservation) => {
    const accessToken = await getAccessToken();
    if (!accessToken) {
      router.push("/login");
      return;
    }

    const response = await fetch(
      `/api/v1/cities/${params.cityId}/restaurants/${params.restaurantId}/reservations/${reservation.reservationId}`,
      {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${accessToken}`,
        },
        body: JSON.stringify({ ...reservation, status: 1 }),
      }
    );

    if (response.ok) {
      setReservations((prevReservations) => {
        if (prevReservations) {
          const index = prevReservations.findIndex(
            (prevReservation) =>
              prevReservation.reservationId === reservation.reservationId
          );

          if (index !== -1) {
            const newReservations = [
              ...prevReservations.slice(0, index),
              { ...reservation, status: 1 },
              ...prevReservations.slice(index + 1),
            ];
            return newReservations;
          }
        }
        return prevReservations;
      });
    }
  };

  const cancelReservation = async (reservation: Reservation) => {
    const accessToken = await getAccessToken();
    if (!accessToken) {
      router.push("/login");
      return;
    }

    const response = await fetch(
      `/api/v1/cities/${params.cityId}/restaurants/${params.restaurantId}/reservations/${reservation.reservationId}`,
      {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${accessToken}`,
        },
        body: JSON.stringify({ ...reservation, status: 2 }),
      }
    );

    if (response.ok) {
      setReservations((prevReservations) => {
        if (prevReservations) {
          const index = prevReservations.findIndex(
            (prevReservation) =>
              prevReservation.reservationId === reservation.reservationId
          );

          if (index !== -1) {
            const newReservations = [
              ...prevReservations.slice(0, index),
              { ...reservation, status: 2 },
              ...prevReservations.slice(index + 1),
            ];
            return newReservations;
          }
        }
        return prevReservations;
      });
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
        case "Admin":
          router.push("/login");
          return;
        case "RegisteredUser":
        case "RestaurantManager":
          setContent(
            <>
              {renderHeader(role)}
              <div className="p-2">
                {reservations?.length === 0 && (
                  <div className="text-sm md:text-base text-center mt-5">
                    No reservations were found for this restaurant
                  </div>
                )}
                {reservations && reservations.length > 0 && (
                  <div className="overflow-x-auto">
                    <table className="table table-xs sm:table-sm md:table-md">
                      <thead>
                        <tr>
                          <th></th>
                          <th>Date</th>
                          <th>Time</th>
                          <th>Party size</th>
                          <th>Status</th>
                          <th>Created at</th>
                          {role === "RestaurantManager" &&
                            reservations.some(
                              (reservation) => reservation.status === 0
                            ) && (
                              <>
                                <th></th>
                                <th></th>
                              </>
                            )}
                        </tr>
                      </thead>
                      <tbody>
                        {reservations.map((reservation, index) => (
                          <tr key={reservation.reservationId}>
                            <th>{index + 1}</th>
                            <td>{reservation.date}</td>
                            <td>
                              {reservation.time
                                .split(":")
                                .slice(0, 2)
                                .join(":")}
                            </td>
                            <td>{reservation.partySize}</td>
                            <td>{getStatusName(reservation.status)}</td>
                            <td>{reservation.createdAt}</td>
                            {role === "RestaurantManager" &&
                              reservation.status === 0 && (
                                <>
                                  <td>
                                    <button
                                      className="btn btn-sm md:btn-md btn-square btn-outline"
                                      onClick={() =>
                                        confirmReservation(reservation)
                                      }
                                    >
                                      <svg
                                        xmlns="http://www.w3.org/2000/svg"
                                        fill="none"
                                        viewBox="0 0 24 24"
                                        strokeWidth="1.5"
                                        stroke="currentColor"
                                        className="w-5 h-5 md:w-6 md:h-6"
                                      >
                                        <path
                                          strokeLinecap="round"
                                          strokeLinejoin="round"
                                          d="M4.5 12.75l6 6 9-13.5"
                                        />
                                      </svg>
                                    </button>
                                  </td>
                                  <td>
                                    <button
                                      className="btn btn-sm md:btn-md btn-square btn-outline"
                                      onClick={() =>
                                        cancelReservation(reservation)
                                      }
                                    >
                                      <svg
                                        xmlns="http://www.w3.org/2000/svg"
                                        fill="none"
                                        viewBox="0 0 24 24"
                                        strokeWidth="1.5"
                                        stroke="currentColor"
                                        className="w-5 h-5 md:w-6 md:h-6"
                                      >
                                        <path
                                          strokeLinecap="round"
                                          strokeLinejoin="round"
                                          d="M6 18L18 6M6 6l12 12"
                                        />
                                      </svg>
                                    </button>
                                  </td>
                                </>
                              )}
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                )}
              </div>
              <Footer />
            </>
          );
          break;
      }
    })();
  }, [reservations]);

  useEffect(() => {
    (async () => {
      const accessToken = await getAccessToken();
      if (!accessToken) {
        router.push("/login");
        return;
      }

      const response = await fetch(
        `/api/v1/cities/${params.cityId}/restaurants/${params.restaurantId}/reservations`,
        {
          method: "GET",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${accessToken}`,
          },
        }
      );

      if (response.ok) {
        const reservations: Reservation[] = await response.json();
        reservations.sort((reservation1, reservation2) => {
          const date1 = new Date(reservation1.createdAt);
          const date2 = new Date(reservation2.createdAt);

          if (date1 > date2) {
            return -1;
          } else if (date1 < date2) {
            return 1;
          } else {
            return 0;
          }
        });
        setReservations(reservations);
      }
    })();
  }, []);

  return content;
}
