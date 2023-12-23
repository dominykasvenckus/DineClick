import Link from "next/link";

export default function Footer() {
  return (
    <div className="footer footer-center p-5 bg-secondary text-secondary-content sticky top-[100vh]">
      <aside>
        <Link href="/" className="btn btn-ghost text-xl">
          DineClick
        </Link>
        <p>© 2023 DineClick</p>
      </aside>
    </div>
  );
}
