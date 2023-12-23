"use client";

import { removeAccessToken, removeRefreshToken } from "../jwt-utils";
import Link from "next/link";

export default function RestaurantManagerHeader() {
  const handleClick = () => {
    const elem = document.activeElement as HTMLElement;
    if (elem) {
      elem?.blur();
    }
  };

  const handleLogout = () => {
    removeAccessToken();
    removeRefreshToken();
  };

  return (
    <div className="navbar bg-base-300">
      <div>
        <div className="dropdown">
          <div tabIndex={0} role="button" className="btn btn-ghost lg:hidden">
            <svg
              xmlns="http://www.w3.org/2000/svg"
              className="h-5 w-5"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth="2"
                d="M4 6h16M4 12h8m-8 6h16"
              />
            </svg>
          </div>
          <ul
            tabIndex={0}
            className="menu menu-sm dropdown-content mt-3 z-[1] p-2 shadow bg-base-300 rounded-box w-52"
          >
            <li onClick={handleClick}>
              <Link href="/restaurants">Restaurants</Link>
            </li>
          </ul>
        </div>
        <Link href="/" className="btn btn-ghost text-xl">
          DineClick
        </Link>
      </div>
      <div className="hidden lg:flex">
        <ul className="menu menu-horizontal px-1">
          <li onClick={handleClick}>
            <Link href="/restaurants">Restaurants</Link>
          </li>
        </ul>
      </div>
      <div className="ml-auto">
        <div className="dropdown dropdown-end">
          <div tabIndex={0} role="button" className="btn btn-ghost btn-square">
            <svg
              xmlns="http://www.w3.org/2000/svg"
              fill="none"
              viewBox="0 0 24 24"
              className="inline-block w-5 h-5 stroke-current"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth="2"
                d="M5 12h.01M12 12h.01M19 12h.01M6 12a1 1 0 11-2 0 1 1 0 012 0zm7 0a1 1 0 11-2 0 1 1 0 012 0zm7 0a1 1 0 11-2 0 1 1 0 012 0z"
              ></path>
            </svg>
          </div>
          <ul
            tabIndex={0}
            className="menu menu-sm dropdown-content mt-3 z-[1] p-2 shadow bg-base-300 rounded-box w-52"
          >
            <li onClick={handleClick}>
              <Link href="/profile">Profile</Link>
            </li>
            <li onClick={handleClick}>
              <Link href="/login" onClick={handleLogout}>
                Logout
              </Link>
            </li>
          </ul>
        </div>
      </div>
    </div>
  );
}
