import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";

export function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl;

  const accessToken = request.cookies.get("neowallet_access_token")?.value;
  const refreshToken = request.cookies.get("neowallet_refresh_token")?.value;
  const isAuthenticated = Boolean(accessToken || refreshToken);

  const isAuthPage = pathname.startsWith("/sign-in") || pathname.startsWith("/sign-up");

  // Redirect unauthenticated users from root to sign-in or dashboard
  if (pathname === "/") {
    if (isAuthenticated) {
      return NextResponse.redirect(new URL("/dashboard", request.url));
    } else {
      return NextResponse.redirect(new URL("/sign-in", request.url));
    }
  }

  // Redirect authenticated users away from auth pages to dashboard
  if (isAuthPage && isAuthenticated) {
    return NextResponse.redirect(new URL("/dashboard", request.url));
  }

  // Protect all dashboard and application pages
  const isPublicAsset =
    pathname.startsWith("/_next") ||
    pathname.startsWith("/api") ||
    pathname.startsWith("/avatars") ||
    pathname.startsWith("/logos") ||
    pathname.startsWith("/screenshots") ||
    pathname.includes("favicon") ||
    pathname.includes("icon.svg");

  if (!isAuthenticated && !isAuthPage && !isPublicAsset) {
    const signInUrl = new URL("/sign-in", request.url);
    signInUrl.searchParams.set("callbackUrl", pathname);
    return NextResponse.redirect(signInUrl);
  }

  return NextResponse.next();
}

export const config = {
  matcher: [
    "/((?!_next/static|_next/image|favicon.ico|.*\\.(?:svg|png|jpg|jpeg|gif|webp)$).*)",
  ],
};
