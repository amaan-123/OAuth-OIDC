import keycloak from "./keycloak";

function App() {
  const callApi = async () => {
    const response = await fetch("https://localhost:7118/api/profile", {
      headers: {
        Authorization: `Bearer ${keycloak.token}`,
      },
    });

    const data = await response.json();

    console.log("API response:", response.status, data);
  };

  const callAdminApi = async () => {
    const response = await fetch("https://localhost:7118/api/profile/admin", {
      headers: {
        Authorization: `Bearer ${keycloak.token}`,
      },
    });

    console.log("Status:", response.status);
    console.log("Response:", await response.text());
  };

  return (
    <div>
      <h1>OAuth/OIDC Lab</h1>

      <p>Logged in as: {keycloak.tokenParsed?.preferred_username}</p>

      <button onClick={callApi}>Call Protected API</button>

      <button onClick={callAdminApi}>Call Admin API</button>

      <button onClick={() => keycloak.logout()}>Logout</button>
    </div>
  );
}

export default App;
