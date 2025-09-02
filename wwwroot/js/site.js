window.tms = {
    fetchJson: async function(url){
        const res = await fetch(url);
        if(!res.ok) throw new Error('Request failed');
        return await res.json();
    }
};


